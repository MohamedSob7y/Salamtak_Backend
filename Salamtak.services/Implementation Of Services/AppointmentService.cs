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
            _notificationService = notificationService;
            _realtimeNotificationService = realtimeNotificationService;
        }

        public async Task<ApiResponse<AppointmentDto>>BookAppointmentAsync(Guid patientUserId, BookAppointmentDto dto)
        {
            var validationResult =
                await _bookValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors
                        .Select(e => e.ErrorMessage));
            }

            var patient = await _unitOfWork
                .Repository<Patient>()
                .FirstOrDefaultAsync(p =>
                    p.UserId == patientUserId);

            if (patient is null)
            {
                throw new NotFoundException(
                    "Patient profile not found.");
            }

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(dto.DoctorId);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor not found.");
            }

            if (!doctor.IsVerified ||
                doctor.VerificationStatus !=
                    DoctorVerificationStatus.Verified)
            {
                throw new ForbiddenException(
                    "Appointments can only be booked with an approved doctor.");
            }

            var clinic = await _unitOfWork
                .Repository<Clinic>()
                .GetByIdAsync(dto.ClinicId);

            if (clinic is null)
            {
                throw new NotFoundException(
                    "Clinic not found.");
            }

            if (clinic.DoctorId != doctor.Id)
            {
                throw new BadRequestException(
                    "Clinic does not belong to the selected doctor.");
            }

            var slot = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .GetByIdAsync(dto.AvailabilitySlotId);

            if (slot is null)
            {
                throw new NotFoundException(
                    "Availability slot not found.");
            }

            if (slot.DoctorId != doctor.Id)
            {
                throw new BadRequestException(
                    "Slot does not belong to the selected doctor.");
            }

            if (slot.ClinicId != clinic.Id)
            {
                throw new BadRequestException(
                    "Slot does not belong to the selected clinic.");
            }

            if (!slot.IsAvailable)
            {
                throw new ConflictException(
                    "This slot is not available.");
            }

            if (slot.StartTime <= DateTime.UtcNow)
            {
                throw new BadRequestException(
                    "Cannot book an expired slot.");
            }

            var alreadyBooked = await _unitOfWork
                .Repository<Appointment>()
                .AnyAsync(a =>
                    a.AvailabilitySlotId ==
                    dto.AvailabilitySlotId);

            if (alreadyBooked)
            {
                throw new ConflictException(
                    "This slot already has an appointment.");
            }

            var bookingMethod = string.Equals(
                dto.BookingMethod,
                "AI",
                StringComparison.OrdinalIgnoreCase)
                    ? BookingMethod.AI
                    : BookingMethod.Direct;

            var appointment = new Appointment
            {
                PatientId = patient.Id,
                DoctorId = doctor.Id,
                ClinicId = clinic.Id,
                AvailabilitySlotId = slot.Id,
                Status = AppointmentStatus.Confirmed,
                BookingMethod = bookingMethod,
                Reason = dto.Reason?.Trim()
            };

            await _unitOfWork
                .Repository<Appointment>()
                .AddAsync(appointment);

            slot.IsAvailable = false;

            _unitOfWork
                .Repository<AvailabilitySlot>()
                .Update(slot);

            await _unitOfWork.SaveChangesAsync();

            appointment.Patient = patient;
            appointment.Doctor = doctor;
            appointment.Clinic = clinic;
            appointment.AvailabilitySlot = slot;

            var patientNotification =
                await _notificationService.CreateAsync(
                    new CreateNotificationDto
                    {
                        UserId = patient.UserId,
                        AppointmentId = appointment.Id,
                        Title = "Appointment Booked",
                        Message =
                            "Your appointment has been booked successfully.",
                        Type = "AppointmentBooked",
                        Channel = "InApp"
                    });

            var doctorNotification =
                await _notificationService.CreateAsync(
                    new CreateNotificationDto
                    {
                        UserId = doctor.UserId,
                        AppointmentId = appointment.Id,
                        Title = "New Appointment",
                        Message =
                            "You have a new appointment booking.",
                        Type = "AppointmentBooked",
                        Channel = "InApp"
                    });

            if (patientNotification.Data is not null)
            {
                await _realtimeNotificationService
                    .SendToPatientAsync(
                        patient.UserId,
                        new RealtimeNotificationDto
                        {
                            NotificationId =
                                patientNotification.Data.NotificationId,
                            Title =
                                patientNotification.Data.Title,
                            Message =
                                patientNotification.Data.Message,
                            Type =
                                patientNotification.Data.Type,
                            CreatedAt =
                                patientNotification.Data.CreatedAt
                        });
            }

            if (doctorNotification.Data is not null)
            {
                await _realtimeNotificationService
                    .SendToDoctorAsync(
                        doctor.UserId,
                        new RealtimeNotificationDto
                        {
                            NotificationId =
                                doctorNotification.Data.NotificationId,
                            Title =
                                doctorNotification.Data.Title,
                            Message =
                                doctorNotification.Data.Message,
                            Type =
                                doctorNotification.Data.Type,
                            CreatedAt =
                                doctorNotification.Data.CreatedAt
                        });
            }

            var result =
                _mapper.Map<AppointmentDto>(appointment);

            return ApiResponse<AppointmentDto>.Ok(
                result,
                "Appointment booked successfully.");
        }

        public async Task<ApiResponse> CancelAppointmentAsync(Guid currentUserId,CancelAppointmentDto dto)
        {
            var validationResult =
                await _cancelValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors
                        .Select(e => e.ErrorMessage));
            }

            var appointment = await _unitOfWork
                .Repository<Appointment>()
                .GetByIdAsync(dto.AppointmentId);

            if (appointment is null)
            {
                throw new NotFoundException(
                    "Appointment not found.");
            }

            if (appointment.Status ==
                AppointmentStatus.Cancelled)
            {
                throw new ConflictException(
                    "Appointment is already cancelled.");
            }

            if (appointment.Status ==
                AppointmentStatus.Completed)
            {
                throw new ConflictException(
                    "Completed appointment cannot be cancelled.");
            }

            var patient = await _unitOfWork
                .Repository<Patient>()
                .FirstOrDefaultAsync(p =>
                    p.UserId == currentUserId);

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(d =>
                    d.UserId == currentUserId);

            var isPatientOwner =
                patient is not null &&
                appointment.PatientId == patient.Id;

            var isDoctorOwner =
                doctor is not null &&
                appointment.DoctorId == doctor.Id;

            if (!isPatientOwner && !isDoctorOwner)
            {
                throw new ForbiddenException(
                    "You are not allowed to cancel this appointment.");
            }

            var slot = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .GetByIdAsync(
                    appointment.AvailabilitySlotId);

            if (slot is null)
            {
                throw new NotFoundException(
                    "Related availability slot not found.");
            }

            if (slot.StartTime <= DateTime.UtcNow)
            {
                throw new BadRequestException(
                    "A started or expired appointment cannot be cancelled.");
            }

            if (slot.StartTime <=
                DateTime.UtcNow.AddHours(24))
            {
                throw new BadRequestException(
                    "Appointment can only be cancelled at least 24 hours in advance.");
            }

            appointment.Status =
                AppointmentStatus.Cancelled;

            appointment.CancelReason =
                dto.CancelReason?.Trim();

            slot.IsAvailable = true;

            _unitOfWork
                .Repository<Appointment>()
                .Update(appointment);

            _unitOfWork
                .Repository<AvailabilitySlot>()
                .Update(slot);

            await _unitOfWork.SaveChangesAsync();

            var appointmentPatient =
                await _unitOfWork
                    .Repository<Patient>()
                    .GetByIdAsync(appointment.PatientId);

            var appointmentDoctor =
                await _unitOfWork
                    .Repository<Doctor>()
                    .GetByIdAsync(appointment.DoctorId);

            if (appointmentPatient is not null)
            {
                var notification =
                    await _notificationService.CreateAsync(
                        new CreateNotificationDto
                        {
                            UserId = appointmentPatient.UserId,
                            AppointmentId = appointment.Id,
                            Title = "Appointment Cancelled",
                            Message =
                                "Your appointment has been cancelled.",
                            Type = "AppointmentCancelled",
                            Channel = "InApp"
                        });

                if (notification.Data is not null)
                {
                    await _realtimeNotificationService
                        .SendToPatientAsync(
                            appointmentPatient.UserId,
                            new RealtimeNotificationDto
                            {
                                NotificationId =
                                    notification.Data.NotificationId,
                                Title = notification.Data.Title,
                                Message = notification.Data.Message,
                                Type = notification.Data.Type,
                                CreatedAt =
                                    notification.Data.CreatedAt
                            });
                }
            }

            if (appointmentDoctor is not null)
            {
                var notification =
                    await _notificationService.CreateAsync(
                        new CreateNotificationDto
                        {
                            UserId = appointmentDoctor.UserId,
                            AppointmentId = appointment.Id,
                            Title = "Appointment Cancelled",
                            Message =
                                "An appointment has been cancelled.",
                            Type = "AppointmentCancelled",
                            Channel = "InApp"
                        });

                if (notification.Data is not null)
                {
                    await _realtimeNotificationService
                        .SendToDoctorAsync(
                            appointmentDoctor.UserId,
                            new RealtimeNotificationDto
                            {
                                NotificationId =
                                    notification.Data.NotificationId,
                                Title = notification.Data.Title,
                                Message = notification.Data.Message,
                                Type = notification.Data.Type,
                                CreatedAt =
                                    notification.Data.CreatedAt
                            });
                }
            }

            return ApiResponse.Ok(
                "Appointment cancelled successfully.");
        }

        public async Task<ApiResponse> CompleteAppointmentAsync(Guid doctorUserId,CompleteAppointmentDto dto)
        {
            var validationResult =
                await _completeValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors
                        .Select(e => e.ErrorMessage));
            }

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(d =>
                    d.UserId == doctorUserId);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor profile not found.");
            }

            var appointment = await _unitOfWork
                .Repository<Appointment>()
                .GetByIdAsync(dto.AppointmentId);

            if (appointment is null)
            {
                throw new NotFoundException(
                    "Appointment not found.");
            }

            if (appointment.DoctorId != doctor.Id)
            {
                throw new ForbiddenException(
                    "You are not allowed to complete this appointment.");
            }

            if (appointment.Status ==
                AppointmentStatus.Cancelled)
            {
                throw new ConflictException(
                    "Cancelled appointment cannot be completed.");
            }

            if (appointment.Status ==
                AppointmentStatus.Completed)
            {
                throw new ConflictException(
                    "Appointment is already completed.");
            }

            var slot = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .GetByIdAsync(
                    appointment.AvailabilitySlotId);

            if (slot is null)
            {
                throw new NotFoundException(
                    "Related availability slot not found.");
            }

            if (DateTime.UtcNow < slot.StartTime)
            {
                throw new BadRequestException(
                    "Appointment cannot be completed before its start time.");
            }

            appointment.Status =
                AppointmentStatus.Completed;

            _unitOfWork
                .Repository<Appointment>()
                .Update(appointment);

            await _unitOfWork.SaveChangesAsync();

            var patient = await _unitOfWork
                .Repository<Patient>()
                .GetByIdAsync(appointment.PatientId);

            if (patient is not null)
            {
                var notification =
                    await _notificationService.CreateAsync(
                        new CreateNotificationDto
                        {
                            UserId = patient.UserId,
                            AppointmentId = appointment.Id,
                            Title = "Appointment Completed",
                            Message =
                                "Your appointment has been marked as completed.",
                            Type = "AppointmentCompleted",
                            Channel = "InApp"
                        });

                if (notification.Data is not null)
                {
                    await _realtimeNotificationService
                        .SendToPatientAsync(
                            patient.UserId,
                            new RealtimeNotificationDto
                            {
                                NotificationId =
                                    notification.Data.NotificationId,
                                Title = notification.Data.Title,
                                Message = notification.Data.Message,
                                Type = notification.Data.Type,
                                CreatedAt =
                                    notification.Data.CreatedAt
                            });
                }
            }

            return ApiResponse.Ok(
                "Appointment completed successfully.");
        }

        public async Task<ApiResponse<AppointmentDetailsDto>>GetByIdAsync(Guid currentUserId, Guid appointmentId)
        {
            var appointment = await _unitOfWork
                .Repository<Appointment>()
                .GetByIdAsync(appointmentId);

            if (appointment is null)
            {
                throw new NotFoundException(
                    "Appointment not found.");
            }

            var patient = await _unitOfWork
                .Repository<Patient>()
                .FirstOrDefaultAsync(p =>
                    p.UserId == currentUserId);

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(d =>
                    d.UserId == currentUserId);

            var isPatientOwner =
                patient is not null &&
                appointment.PatientId == patient.Id;

            var isDoctorOwner =
                doctor is not null &&
                appointment.DoctorId == doctor.Id;

            if (!isPatientOwner && !isDoctorOwner)
            {
                throw new ForbiddenException(
                    "You are not allowed to view this appointment.");
            }

            await LoadAppointmentDataAsync(appointment);

            var result =
                _mapper.Map<AppointmentDetailsDto>(appointment);

            return ApiResponse<AppointmentDetailsDto>.Ok(
                result,
                "Appointment retrieved successfully.");
        }
        public async Task<ApiResponse<IReadOnlyList<PatientAppointmentDto>>>GetPatientAppointmentsAsync(Guid patientUserId)
        {
            var patient = await _unitOfWork
                .Repository<Patient>()
                .FirstOrDefaultAsync(p =>
                    p.UserId == patientUserId);

            if (patient is null)
            {
                throw new NotFoundException(
                    "Patient profile not found.");
            }

            var appointments = await _unitOfWork
                .Repository<Appointment>()
                .GetAllAsync(a =>
                    a.PatientId == patient.Id);

            foreach (var appointment in appointments)
            {
                await LoadAppointmentDataAsync(appointment);
            }

            var orderedAppointments = appointments
                .OrderByDescending(a =>
                    a.AvailabilitySlot?.StartTime ??
                    a.CreatedAt)
                .ToList();

            var result = _mapper.Map<
                IReadOnlyList<PatientAppointmentDto>>(
                orderedAppointments);

            return ApiResponse<
                IReadOnlyList<PatientAppointmentDto>>.Ok(
                result,
                "Patient appointments retrieved successfully.");
        }
        public async Task<ApiResponse<IReadOnlyList<DoctorAppointmentDto>>>GetDoctorAppointmentsAsync(Guid doctorUserId)
        {
            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(d => d.UserId == doctorUserId);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor profile not found.");
            }

            var appointments = await _unitOfWork
                .Repository<Appointment>()
                .GetAllAsync(a => a.DoctorId == doctor.Id);

            foreach (var appointment in appointments)
            {
                await LoadAppointmentDataAsync(appointment);
            }

            var orderedAppointments = appointments
                .OrderByDescending(a =>
                    a.AvailabilitySlot?.StartTime ??
                    a.CreatedAt)
                .ToList();

            var result = _mapper.Map<
                IReadOnlyList<DoctorAppointmentDto>>(
                    orderedAppointments);

            return ApiResponse<
                IReadOnlyList<DoctorAppointmentDto>>.Ok(
                    result,
                    "Doctor appointments retrieved successfully.");
        }

        #region Helper Methods

        private async Task LoadAppointmentDataAsync(
            Appointment appointment)
        {
            appointment.Patient = await _unitOfWork
                .Repository<Patient>()
                .GetByIdAsync(appointment.PatientId);

            if (appointment.Patient is not null)
            {
                appointment.Patient.User = await _unitOfWork
                    .Repository<User>()
                    .GetByIdAsync(appointment.Patient.UserId);
            }

            appointment.Doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(appointment.DoctorId);

            if (appointment.Doctor is not null)
            {
                appointment.Doctor.User = await _unitOfWork
                    .Repository<User>()
                    .GetByIdAsync(appointment.Doctor.UserId);

                appointment.Doctor.Specialty = await _unitOfWork
                    .Repository<Specialty>()
                    .GetByIdAsync(appointment.Doctor.SpecialtyId);
            }

            appointment.Clinic = await _unitOfWork
                .Repository<Clinic>()
                .GetByIdAsync(appointment.ClinicId);

            appointment.AvailabilitySlot = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .GetByIdAsync(appointment.AvailabilitySlotId);
        }

        #endregion
    }
}
