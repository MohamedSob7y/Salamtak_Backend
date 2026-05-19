using AutoMapper;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.Appointments;
using Salamtak.Shared.DTOs.Notifications;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Salamtak.services.Implementation_Of_Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<BookAppointmentDto> _bookValidator;
        private readonly IValidator<CancelAppointmentDto> _cancelValidator;
        private readonly IValidator<CompleteAppointmentDto> _completeValidator;
        private readonly INotificationService _notificationService;
        private readonly IRealtimeNotificationService _realtimeNotificationService;
        public AppointmentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<BookAppointmentDto> bookValidator,
            IValidator<CancelAppointmentDto> cancelValidator,
            IValidator<CompleteAppointmentDto> completeValidator,
            INotificationService notificationService,
            IRealtimeNotificationService realtimeNotificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _bookValidator = bookValidator;
            _cancelValidator = cancelValidator;
            _completeValidator = completeValidator;
            _notificationService= notificationService;
            _realtimeNotificationService = realtimeNotificationService;
        }

        public async Task<ApiResponse<AppointmentDto>> BookAppointmentAsync(Guid patientId, BookAppointmentDto dto)
        {
            var validationResult = await _bookValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var patient = await _unitOfWork.Repository<Patient>().GetByIdAsync(patientId);

            if (patient is null)
                throw new NotFoundException("Patient not found.");

            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(dto.DoctorId);

            if (doctor is null)
                throw new NotFoundException("Doctor not found.");

            if (!doctor.IsVerified)
                throw new ForbiddenException("Doctor is not verified.");

            var clinic = await _unitOfWork.Repository<Clinic>().GetByIdAsync(dto.ClinicId);

            if (clinic is null)
                throw new NotFoundException("Clinic not found.");

            if (clinic.DoctorId != dto.DoctorId)
                throw new BadRequestException("Clinic does not belong to this doctor.");

            var slot = await _unitOfWork.Repository<AvailabilitySlot>().GetByIdAsync(dto.AvailabilitySlotId);

            if (slot is null)
                throw new NotFoundException("Availability slot not found.");

            if (slot.DoctorId != dto.DoctorId)
                throw new BadRequestException("Slot does not belong to this doctor.");

            if (slot.ClinicId != dto.ClinicId)
                throw new BadRequestException("Slot does not belong to this clinic.");

            if (!slot.IsAvailable)
                throw new ConflictException("This slot is already booked.");

            if (slot.StartTime <= DateTime.UtcNow)
                throw new BadRequestException("Cannot book an expired slot.");

            var alreadyBooked = await _unitOfWork
                .Repository<Appointment>()
                .AnyAsync(a => a.AvailabilitySlotId == dto.AvailabilitySlotId);

            if (alreadyBooked)
                throw new ConflictException("This slot already has an appointment.");

            var bookingMethod = dto.BookingMethod.Equals("AI", StringComparison.OrdinalIgnoreCase)
                 ? BookingMethod.AI
                 : BookingMethod.Direct;

            var appointment = new Appointment
            {
                PatientId = patientId,
                DoctorId = dto.DoctorId,
                ClinicId = dto.ClinicId,
                AvailabilitySlotId = dto.AvailabilitySlotId,
                Status = AppointmentStatus.Confirmed,
                BookingMethod = bookingMethod,
                Reason = dto.Reason?.Trim()
            };

            await _unitOfWork.Repository<Appointment>().AddAsync(appointment);

            slot.IsAvailable = false;

            _unitOfWork.Repository<AvailabilitySlot>().Update(slot);

            await _unitOfWork.SaveChangesAsync();

            appointment.AvailabilitySlot = slot;

            // Save notification for patient in database
            var patientNotification = await _notificationService.CreateAsync(new CreateNotificationDto
            {
                UserId = patient.UserId,
                AppointmentId = appointment.Id,
                Title = "Appointment Booked",
                Message = "Your appointment has been booked successfully.",
                Type = "AppointmentBooked",
                Channel = "InApp"
            });

            // Save notification for doctor in database
            var doctorNotification = await _notificationService.CreateAsync(new CreateNotificationDto
            {
                UserId = doctor.UserId,
                AppointmentId = appointment.Id,
                Title = "New Appointment",
                Message = "You have a new appointment booking.",
                Type = "AppointmentBooked",
                Channel = "InApp"
            });

            // Send realtime notification to patient
            if (patientNotification.Data is not null)
            {
                await _realtimeNotificationService.SendToPatientAsync(patient.UserId, new RealtimeNotificationDto
                {
                    NotificationId = patientNotification.Data.NotificationId,
                    Title = patientNotification.Data.Title,
                    Message = patientNotification.Data.Message,
                    Type = patientNotification.Data.Type,
                    CreatedAt = patientNotification.Data.CreatedAt
                });
            }

            // Send realtime notification to doctor
            if (doctorNotification.Data is not null)
            {
                await _realtimeNotificationService.SendToDoctorAsync(doctor.UserId, new RealtimeNotificationDto
                {
                    NotificationId = doctorNotification.Data.NotificationId,
                    Title = doctorNotification.Data.Title,
                    Message = doctorNotification.Data.Message,
                    Type = doctorNotification.Data.Type,
                    CreatedAt = doctorNotification.Data.CreatedAt
                });
            }

            var result = _mapper.Map<AppointmentDto>(appointment);

            return ApiResponse<AppointmentDto>.Ok(result, "Appointment booked successfully.");
        }

        public async Task<ApiResponse> CancelAppointmentAsync(Guid userId, CancelAppointmentDto dto)
        {
            var validationResult = await _cancelValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(dto.AppointmentId);

            if (appointment is null)
                throw new NotFoundException("Appointment not found.");

            if (appointment.Status == AppointmentStatus.Cancelled)
                throw new ConflictException("Appointment is already cancelled.");

            if (appointment.Status == AppointmentStatus.Completed)
                throw new ConflictException("Completed appointment cannot be cancelled.");

            var slot = await _unitOfWork.Repository<AvailabilitySlot>().GetByIdAsync(appointment.AvailabilitySlotId);

            if (slot is null)
                throw new NotFoundException("Related slot not found.");

            var patient = await _unitOfWork.Repository<Patient>()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var doctor = await _unitOfWork.Repository<Doctor>()
                .FirstOrDefaultAsync(d => d.UserId == userId);

            var isPatientOwner = patient is not null && appointment.PatientId == patient.Id;
            var isDoctorOwner = doctor is not null && appointment.DoctorId == doctor.Id;

            if (!isPatientOwner && !isDoctorOwner)
                throw new ForbiddenException("You are not allowed to cancel this appointment.");

            if (slot.StartTime <= DateTime.UtcNow.AddHours(24))
                throw new BadRequestException("Appointment can only be cancelled at least 24 hours in advance.");

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancelReason = dto.CancelReason?.Trim();

            slot.IsAvailable = true;

            _unitOfWork.Repository<Appointment>().Update(appointment);
            _unitOfWork.Repository<AvailabilitySlot>().Update(slot);

            await _unitOfWork.SaveChangesAsync();

            var appointmentPatient = await _unitOfWork.Repository<Patient>()
                .GetByIdAsync(appointment.PatientId);

            var appointmentDoctor = await _unitOfWork.Repository<Doctor>()
                .GetByIdAsync(appointment.DoctorId);

            if (appointmentPatient is not null)
            {
                var patientNotification = await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    UserId = appointmentPatient.UserId,
                    AppointmentId = appointment.Id,
                    Title = "Appointment Cancelled",
                    Message = "Your appointment has been cancelled.",
                    Type = "AppointmentCancelled",
                    Channel = "InApp"
                });

                if (patientNotification.Data is not null)
                {
                    await _realtimeNotificationService.SendToPatientAsync(
                        appointmentPatient.UserId,
                        new RealtimeNotificationDto
                        {
                            NotificationId = patientNotification.Data.NotificationId,
                            Title = patientNotification.Data.Title,
                            Message = patientNotification.Data.Message,
                            Type = patientNotification.Data.Type,
                            CreatedAt = patientNotification.Data.CreatedAt
                        });
                }
            }

            if (appointmentDoctor is not null)
            {
                var doctorNotification = await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    UserId = appointmentDoctor.UserId,
                    AppointmentId = appointment.Id,
                    Title = "Appointment Cancelled",
                    Message = "An appointment has been cancelled.",
                    Type = "AppointmentCancelled",
                    Channel = "InApp"
                });

                if (doctorNotification.Data is not null)
                {
                    await _realtimeNotificationService.SendToDoctorAsync(
                        appointmentDoctor.UserId,
                        new RealtimeNotificationDto
                        {
                            NotificationId = doctorNotification.Data.NotificationId,
                            Title = doctorNotification.Data.Title,
                            Message = doctorNotification.Data.Message,
                            Type = doctorNotification.Data.Type,
                            CreatedAt = doctorNotification.Data.CreatedAt
                        });
                }
            }

            return ApiResponse.Ok("Appointment cancelled successfully.");
        }

        public async Task<ApiResponse> CompleteAppointmentAsync(Guid doctorId, CompleteAppointmentDto dto)
        {
            var validationResult = await _completeValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(dto.AppointmentId);

            if (appointment is null)
                throw new NotFoundException("Appointment not found.");

            if (appointment.DoctorId != doctorId)
                throw new ForbiddenException("You are not allowed to complete this appointment.");

            if (appointment.Status == AppointmentStatus.Cancelled)
                throw new ConflictException("Cancelled appointment cannot be completed.");

            if (appointment.Status == AppointmentStatus.Completed)
                throw new ConflictException("Appointment is already completed.");

            appointment.Status = AppointmentStatus.Completed;

            _unitOfWork.Repository<Appointment>().Update(appointment);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Appointment completed successfully.");
        }

        public async Task<ApiResponse<AppointmentDetailsDto>> GetByIdAsync(Guid appointmentId)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(appointmentId);

            if (appointment is null)
                throw new NotFoundException("Appointment not found.");

            var result = _mapper.Map<AppointmentDetailsDto>(appointment);

            return ApiResponse<AppointmentDetailsDto>.Ok(result);
        }

        public async Task<ApiResponse<IReadOnlyList<PatientAppointmentDto>>> GetPatientAppointmentsAsync(Guid patientId)
        {
            var appointments = await _unitOfWork
                .Repository<Appointment>()
                .GetAllAsync(a => a.PatientId == patientId);

            var result = _mapper.Map<IReadOnlyList<PatientAppointmentDto>>(appointments);

            return ApiResponse<IReadOnlyList<PatientAppointmentDto>>.Ok(result);
        }

        public async Task<ApiResponse<IReadOnlyList<DoctorAppointmentDto>>> GetDoctorAppointmentsAsync(Guid doctorId)
        {
            var appointments = await _unitOfWork
                .Repository<Appointment>()
                .GetAllAsync(a => a.DoctorId == doctorId);

            var result = _mapper.Map<IReadOnlyList<DoctorAppointmentDto>>(appointments);

            return ApiResponse<IReadOnlyList<DoctorAppointmentDto>>.Ok(result);
        }
    }
}
