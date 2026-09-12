using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.Appointments;
using Salamtak.Shared.DTOs.Notifications;
using Salamtak.Shared.Responses;

namespace Salamtak.services.Implementation_Of_Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private readonly IValidator<BookAppointmentDto>
            _bookValidator;

        private readonly IValidator<CancelAppointmentDto>
            _cancelValidator;

        private readonly IValidator<CompleteAppointmentDto>
            _completeValidator;

        private readonly INotificationService
            _notificationService;

        private readonly ILogger<AppointmentService>
            _logger;

        public AppointmentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<BookAppointmentDto> bookValidator,
            IValidator<CancelAppointmentDto> cancelValidator,
            IValidator<CompleteAppointmentDto> completeValidator,
            INotificationService notificationService,
            ILogger<AppointmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;

            _bookValidator = bookValidator;
            _cancelValidator = cancelValidator;
            _completeValidator = completeValidator;

            _notificationService = notificationService;
            _logger = logger;
        }
        public async Task<int> CancelExpiredPendingAppointmentsAsync()
        {
            var now = DateTime.UtcNow;

            var pendingAppointments = await _unitOfWork
                .Repository<Appointment>()
                .GetAllAsync(appointment =>
                    appointment.Status == AppointmentStatus.Pending);

            var cancelledCount = 0;

            foreach (var appointment in pendingAppointments)
            {
                var slot = await _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .GetByIdAsync(
                        appointment.AvailabilitySlotId);

                if (slot is null)
                {
                    continue;
                }

                if (slot.EndTime >= now)
                {
                    continue;
                }

                appointment.Status =
                    AppointmentStatus.Cancelled;

                appointment.CancelReason =
                    "Appointment expired before doctor confirmation.";

                slot.IsAvailable = false;

                _unitOfWork
                    .Repository<Appointment>()
                    .Update(appointment);

                _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .Update(slot);

                cancelledCount++;
            }

            if (cancelledCount > 0)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return cancelledCount;
        }
        public async Task<ApiResponse<AppointmentDto>>BookAppointmentAsync(Guid patientUserId,BookAppointmentDto dto)
        {
            var validationResult =
                await _bookValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors.Select(
                        error => error.ErrorMessage));
            }

            var patient = await _unitOfWork
                .Repository<Patient>()
                .FirstOrDefaultAsync(patient =>
                    patient.UserId == patientUserId);

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

            var doctorClinicExists = await _unitOfWork
                .Repository<DoctorClinic>()
                .AnyAsync(relation =>
                    relation.DoctorId == doctor.Id &&
                    relation.ClinicId == clinic.Id);

            if (!doctorClinicExists)
            {
                throw new BadRequestException(
                    "The selected doctor is not registered at the selected clinic.");
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

            var hasActiveAppointmentForSlot =
                await _unitOfWork
                    .Repository<Appointment>()
                    .AnyAsync(appointment =>
                        appointment.AvailabilitySlotId ==
                            slot.Id &&
                        (
                            appointment.Status ==
                                AppointmentStatus.Pending ||
                            appointment.Status ==
                                AppointmentStatus.Confirmed
                        ));

            if (hasActiveAppointmentForSlot)
            {
                throw new ConflictException(
                    "This slot already has an active appointment.");
            }

            /*
             * يمنع المريض من حجز موعدين متداخلين،
             * سواء مع نفس الطبيب أو طبيب مختلف.
             */
            var patientActiveAppointments =
                await _unitOfWork
                    .Repository<Appointment>()
                    .GetAllAsync(appointment =>
                        appointment.PatientId == patient.Id &&
                        (
                            appointment.Status ==
                                AppointmentStatus.Pending ||
                            appointment.Status ==
                                AppointmentStatus.Confirmed
                        ));

            foreach (var patientAppointment
                     in patientActiveAppointments)
            {
                var existingPatientSlot =
                    await _unitOfWork
                        .Repository<AvailabilitySlot>()
                        .GetByIdAsync(
                            patientAppointment
                                .AvailabilitySlotId);

                if (existingPatientSlot is null)
                {
                    continue;
                }

                var hasPatientTimeConflict =
                    slot.StartTime <
                        existingPatientSlot.EndTime &&
                    slot.EndTime >
                        existingPatientSlot.StartTime;

                if (hasPatientTimeConflict)
                {
                    throw new ConflictException(
                        "You already have another appointment during this time.");
                }
            }

            var bookingMethod =
                string.Equals(
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

                /*
                 * الحجز يبدأ Pending،
                 * والطبيب هو الذي يؤكده.
                 */
                Status = AppointmentStatus.Pending,

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

            await TrySendNotificationsAsync(
                () => SendBookedNotificationsAsync(
                    appointment,
                    patient,
                    doctor),
                appointment.Id,
                "booking");

            var result =
                _mapper.Map<AppointmentDto>(
                    appointment);

            return ApiResponse<AppointmentDto>.Ok(
                result,
                "Appointment booked successfully and is awaiting doctor confirmation.");
        }

        public async Task<ApiResponse>ConfirmAppointmentAsync(Guid doctorUserId,Guid appointmentId)
        {
            if (appointmentId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Appointment id is required.");
            }

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(doctor =>
                    doctor.UserId == doctorUserId);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor profile not found.");
            }

            var appointment = await _unitOfWork
                .Repository<Appointment>()
                .GetByIdAsync(appointmentId);

            if (appointment is null)
            {
                throw new NotFoundException(
                    "Appointment not found.");
            }

            if (appointment.DoctorId != doctor.Id)
            {
                throw new ForbiddenException(
                    "You are not allowed to confirm this appointment.");
            }

            if (appointment.Status ==
                AppointmentStatus.Confirmed)
            {
                throw new ConflictException(
                    "Appointment is already confirmed.");
            }

            if (appointment.Status !=
                AppointmentStatus.Pending)
            {
                throw new ConflictException(
                    "Only pending appointments can be confirmed.");
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
                throw new ConflictException(
                    "An appointment that has already started cannot be confirmed.");
            }

            appointment.Status =
                AppointmentStatus.Confirmed;

            _unitOfWork
                .Repository<Appointment>()
                .Update(appointment);

            await _unitOfWork.SaveChangesAsync();

            await TrySendNotificationsAsync(
                () => SendConfirmedNotificationAsync(
                    appointment),
                appointment.Id,
                "confirmation");

            return ApiResponse.Ok(
                "Appointment confirmed successfully.");
        }

        public async Task<ApiResponse>CancelAppointmentAsync(Guid currentUserId,CancelAppointmentDto dto)
        {
            var validationResult =
                await _cancelValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors.Select(
                        error => error.ErrorMessage));
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

            if (appointment.Status ==
                AppointmentStatus.NoShow)
            {
                throw new ConflictException(
                    "No-show appointment cannot be cancelled.");
            }

            var patient = await _unitOfWork
                .Repository<Patient>()
                .FirstOrDefaultAsync(item =>
                    item.UserId == currentUserId);

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(item =>
                    item.UserId == currentUserId);

            var isPatientOwner =
                patient is not null &&
                appointment.PatientId ==
                patient.Id;

            var isDoctorOwner =
                doctor is not null &&
                appointment.DoctorId ==
                doctor.Id;

            if (!isPatientOwner &&
                !isDoctorOwner)
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

            /*
             * إذا المريض هو الذي ألغى:
             * ترجع الـSlot متاحة.
             *
             * إذا الطبيب هو الذي ألغى:
             * تظل غير متاحة، لأن الطبيب ألغى وقت العمل نفسه.
             */
            slot.IsAvailable = isPatientOwner;

            _unitOfWork
                .Repository<Appointment>()
                .Update(appointment);

            _unitOfWork
                .Repository<AvailabilitySlot>()
                .Update(slot);

            await _unitOfWork.SaveChangesAsync();

            await TrySendNotificationsAsync(
                () => SendCancelledNotificationsAsync(
                    appointment,
                    isDoctorOwner),
                appointment.Id,
                "cancellation");

            return ApiResponse.Ok(
                "Appointment cancelled successfully.");
        }

        public async Task<ApiResponse>
            CompleteAppointmentAsync(
                Guid doctorUserId,
                CompleteAppointmentDto dto)
        {
            var validationResult =
                await _completeValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors.Select(
                        error => error.ErrorMessage));
            }

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(doctor =>
                    doctor.UserId == doctorUserId);

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

            /*
             * يمنع إكمال:
             * Pending
             * Cancelled
             * Completed
             * NoShow
             */
            if (appointment.Status !=
                AppointmentStatus.Confirmed)
            {
                throw new ConflictException(
                    "Only confirmed appointments can be completed.");
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

            if (DateTime.UtcNow <
                slot.StartTime)
            {
                throw new BadRequestException(
                    "Appointment cannot be completed before its start time.");
            }

            var medicalReport = await _unitOfWork
                .Repository<MedicalReport>()
                .FirstOrDefaultAsync(report =>
                    report.PatientId ==
                    appointment.PatientId);

            if (medicalReport is null)
            {
                throw new NotFoundException(
                    "Patient medical report not found.");
            }

            var medicalReportEntry =
                await _unitOfWork
                    .Repository<MedicalReportEntry>()
                    .FirstOrDefaultAsync(entry =>
                        entry.AppointmentId ==
                        appointment.Id);

            var notes =
                string.IsNullOrWhiteSpace(dto.Notes)
                    ? null
                    : dto.Notes.Trim();

            if (medicalReportEntry is null)
            {
                medicalReportEntry =
                    new MedicalReportEntry
                    {
                        MedicalReportId =
                            medicalReport.Id,

                        AppointmentId =
                            appointment.Id,

                        DoctorId =
                            doctor.Id,

                        Notes =
                            notes
                    };

                await _unitOfWork
                    .Repository<MedicalReportEntry>()
                    .AddAsync(medicalReportEntry);
            }
            else
            {
                if (medicalReportEntry.DoctorId !=
                    doctor.Id)
                {
                    throw new ConflictException(
                        "The medical report entry belongs to another doctor.");
                }

                if (medicalReportEntry
                        .MedicalReportId !=
                    medicalReport.Id)
                {
                    throw new ConflictException(
                        "The medical report entry does not belong to the patient's medical report.");
                }

                medicalReportEntry.Notes =
                    notes;

                _unitOfWork
                    .Repository<MedicalReportEntry>()
                    .Update(medicalReportEntry);
            }

            appointment.Status =
                AppointmentStatus.Completed;

            /*
             * الموعد تم استخدامه بالفعل،
             * فلا تعود الـSlot متاحة.
             */
            slot.IsAvailable = false;

            _unitOfWork
                .Repository<Appointment>()
                .Update(appointment);

            _unitOfWork
                .Repository<AvailabilitySlot>()
                .Update(slot);

            /*
             * يحفظ في عملية واحدة:
             * Appointment Status
             * Slot Status
             * MedicalReportEntry
             */
            await _unitOfWork.SaveChangesAsync();

            await TrySendNotificationsAsync(
                () => SendCompletedNotificationAsync(
                    appointment),
                appointment.Id,
                "completion");

            return ApiResponse.Ok(
                "Appointment completed successfully.");
        }

        public async Task<ApiResponse>MarkAsNoShowAsync(Guid doctorUserId,Guid appointmentId)
        {
            if (appointmentId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Appointment id is required.");
            }

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(doctor =>
                    doctor.UserId == doctorUserId);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor profile not found.");
            }

            var appointment = await _unitOfWork
                .Repository<Appointment>()
                .GetByIdAsync(appointmentId);

            if (appointment is null)
            {
                throw new NotFoundException(
                    "Appointment not found.");
            }

            if (appointment.DoctorId != doctor.Id)
            {
                throw new ForbiddenException(
                    "You are not allowed to update this appointment.");
            }

            if (appointment.Status ==
                AppointmentStatus.NoShow)
            {
                throw new ConflictException(
                    "Appointment is already marked as no-show.");
            }

            if (appointment.Status !=
                AppointmentStatus.Confirmed)
            {
                throw new ConflictException(
                    "Only confirmed appointments can be marked as no-show.");
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

            /*
             * لا يمكن اعتبار المريض NoShow
             * قبل انتهاء وقت الموعد.
             */
            if (slot.EndTime >
                DateTime.UtcNow)
            {
                throw new ConflictException(
                    "Appointment cannot be marked as no-show before its end time.");
            }

            appointment.Status =
                AppointmentStatus.NoShow;

            slot.IsAvailable = false;

            _unitOfWork
                .Repository<Appointment>()
                .Update(appointment);

            _unitOfWork
                .Repository<AvailabilitySlot>()
                .Update(slot);

            await _unitOfWork.SaveChangesAsync();

            await TrySendNotificationsAsync(
                () => SendNoShowNotificationAsync(
                    appointment),
                appointment.Id,
                "no-show");

            return ApiResponse.Ok(
                "Appointment marked as no-show successfully.");
        }

        public async Task<ApiResponse<AppointmentDetailsDto>>GetByIdAsync(Guid currentUserId,Guid appointmentId)
        {
            var appointment = await _unitOfWork
                .Repository<Appointment>()
                .GetByIdAsync(appointmentId);

            if (appointment is null)
            {
                throw new NotFoundException(
                    "Appointment not found.");
            }

            await EnsureAppointmentAccessAsync(
                currentUserId,
                appointment);

            await LoadAppointmentNavigationDataAsync(
                appointment);

            var result =
                _mapper.Map<AppointmentDetailsDto>(
                    appointment);

            return ApiResponse<
                AppointmentDetailsDto>.Ok(
                    result);
        }

        public async Task<ApiResponse<IReadOnlyList<PatientAppointmentDto>>>GetPatientAppointmentsAsync(Guid patientUserId)
        {
            var patient = await _unitOfWork
                .Repository<Patient>()
                .FirstOrDefaultAsync(patient =>
                    patient.UserId ==
                    patientUserId);

            if (patient is null)
            {
                throw new NotFoundException(
                    "Patient profile not found.");
            }

            var appointments = await _unitOfWork
                .Repository<Appointment>()
                .GetAllAsync(appointment =>
                    appointment.PatientId ==
                    patient.Id);

            var result =
                new List<PatientAppointmentDto>();

            foreach (var appointment in appointments
                         .OrderByDescending(
                             appointment =>
                                 appointment.CreatedAt))
            {
                var doctor = await _unitOfWork
                    .Repository<Doctor>()
                    .GetByIdAsync(
                        appointment.DoctorId);

                var doctorUser =
                    doctor is null
                        ? null
                        : await _unitOfWork
                            .Repository<User>()
                            .GetByIdAsync(
                                doctor.UserId);

                var specialty =
                    doctor is null
                        ? null
                        : await _unitOfWork
                            .Repository<Specialty>()
                            .GetByIdAsync(
                                doctor.SpecialtyId);

                var slot = await _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .GetByIdAsync(
                        appointment
                            .AvailabilitySlotId);

                result.Add(
                    new PatientAppointmentDto
                    {
                        AppointmentId =
                            appointment.Id,

                        DoctorName =
                            doctorUser?.FullName ??
                            string.Empty,

                        SpecialtyName =
                            specialty?.Name ??
                            string.Empty,

                        AppointmentDate =
                            slot?.StartTime ??
                            appointment.CreatedAt,

                        Status =
                            appointment.Status
                                .ToString()
                    });
            }

            return ApiResponse<
                IReadOnlyList<
                    PatientAppointmentDto>>.Ok(
                        result);
        }

        public async Task<ApiResponse<IReadOnlyList<DoctorAppointmentDto>>>GetDoctorAppointmentsAsync(Guid doctorUserId)
        {
            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(doctor =>
                    doctor.UserId ==
                    doctorUserId);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor profile not found.");
            }

            var appointments = await _unitOfWork
                .Repository<Appointment>()
                .GetAllAsync(appointment =>
                    appointment.DoctorId ==
                    doctor.Id);

            var result =
                new List<DoctorAppointmentDto>();

            foreach (var appointment in appointments
                         .OrderByDescending(
                             appointment =>
                                 appointment.CreatedAt))
            {
                var patient = await _unitOfWork
                    .Repository<Patient>()
                    .GetByIdAsync(
                        appointment.PatientId);

                var patientUser =
                    patient is null
                        ? null
                        : await _unitOfWork
                            .Repository<User>()
                            .GetByIdAsync(
                                patient.UserId);

                var slot = await _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .GetByIdAsync(
                        appointment
                            .AvailabilitySlotId);

                result.Add(
                    new DoctorAppointmentDto
                    {
                        AppointmentId =
                            appointment.Id,

                        PatientName =
                            patientUser?.FullName ??
                            string.Empty,

                        AppointmentDate =
                            slot?.StartTime ??
                            appointment.CreatedAt,

                        Status =
                            appointment.Status
                                .ToString(),

                        Reason =
                            appointment.Reason ??
                            string.Empty
                    });
            }

            return ApiResponse<
                IReadOnlyList<
                    DoctorAppointmentDto>>.Ok(
                        result);
        }

        private async Task EnsureAppointmentAccessAsync(Guid currentUserId,Appointment appointment)
        {
            var patient = await _unitOfWork
                .Repository<Patient>()
                .FirstOrDefaultAsync(patient =>
                    patient.UserId ==
                    currentUserId);

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(doctor =>
                    doctor.UserId ==
                    currentUserId);

            var canAccessAsPatient =
                patient is not null &&
                appointment.PatientId ==
                patient.Id;

            var canAccessAsDoctor =
                doctor is not null &&
                appointment.DoctorId ==
                doctor.Id;

            if (!canAccessAsPatient &&
                !canAccessAsDoctor)
            {
                throw new ForbiddenException(
                    "You are not allowed to view this appointment.");
            }
        }

        private async Task LoadAppointmentNavigationDataAsync(Appointment appointment)
        {
            var patient = await _unitOfWork
                .Repository<Patient>()
                .GetByIdAsync(
                    appointment.PatientId);

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(
                    appointment.DoctorId);

            var clinic = await _unitOfWork
                .Repository<Clinic>()
                .GetByIdAsync(
                    appointment.ClinicId);

            var slot = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .GetByIdAsync(
                    appointment.AvailabilitySlotId);

            if (patient is not null)
            {
                var user = await _unitOfWork
                    .Repository<User>()
                    .GetByIdAsync(
                        patient.UserId);

                if (user is not null)
                {
                    patient.User = user;
                }

                appointment.Patient = patient;
            }

            if (doctor is not null)
            {
                var user = await _unitOfWork
                    .Repository<User>()
                    .GetByIdAsync(
                        doctor.UserId);

                var specialty = await _unitOfWork
                    .Repository<Specialty>()
                    .GetByIdAsync(
                        doctor.SpecialtyId);

                if (user is not null)
                {
                    doctor.User = user;
                }

                if (specialty is not null)
                {
                    doctor.Specialty = specialty;
                }

                appointment.Doctor = doctor;
            }

            if (clinic is not null)
            {
                appointment.Clinic = clinic;
            }

            if (slot is not null)
            {
                appointment.AvailabilitySlot =
                    slot;
            }
        }

        private async Task SendBookedNotificationsAsync(Appointment appointment,Patient patient,Doctor doctor)
        {
            await _notificationService.CreateAsync(
                new CreateNotificationDto
                {
                    UserId = patient.UserId,
                    AppointmentId = appointment.Id,

                    Title =
                        "Appointment Booked",

                    Message =
                        "Your appointment has been booked successfully and is awaiting doctor confirmation.",

                    Type =
                        NotificationType
                            .AppointmentBooked
                            .ToString(),

                    
                });

            await _notificationService.CreateAsync(
                new CreateNotificationDto
                {
                    UserId = doctor.UserId,
                    AppointmentId = appointment.Id,

                    Title =
                        "New Appointment Request",

                    Message =
                        "A patient has requested a new appointment with you.",

                    Type =
                        NotificationType
                            .AppointmentBooked
                            .ToString(),

                   
                });
        }

        private async Task SendConfirmedNotificationAsync(Appointment appointment)
        {
            var patient = await _unitOfWork
                .Repository<Patient>()
                .GetByIdAsync(
                    appointment.PatientId);

            if (patient is null)
            {
                return;
            }

            await _notificationService.CreateAsync(
                new CreateNotificationDto
                {
                    UserId = patient.UserId,
                    AppointmentId = appointment.Id,

                    Title =
                        "Appointment Confirmed",

                    Message =
                        "Your appointment has been confirmed by the doctor.",

                    Type =
                        NotificationType
                            .AppointmentConfirmed
                            .ToString(),

                   
                });
        }

        private async Task SendCancelledNotificationsAsync(Appointment appointment,bool cancelledByDoctor)
        {
            var patient = await _unitOfWork
                .Repository<Patient>()
                .GetByIdAsync(
                    appointment.PatientId);

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(
                    appointment.DoctorId);

            if (patient is not null)
            {
                await _notificationService.CreateAsync(
                    new CreateNotificationDto
                    {
                        UserId = patient.UserId,
                        AppointmentId =
                            appointment.Id,

                        Title =
                            "Appointment Cancelled",

                        Message = cancelledByDoctor
                            ? "Your appointment has been cancelled by the doctor."
                            : "Your appointment has been cancelled successfully.",

                        Type =
                            NotificationType
                                .AppointmentCancelled
                                .ToString(),

                        
                    });
            }

            if (doctor is not null)
            {
                await _notificationService.CreateAsync(
                    new CreateNotificationDto
                    {
                        UserId = doctor.UserId,
                        AppointmentId =
                            appointment.Id,

                        Title =
                            "Appointment Cancelled",

                        Message = cancelledByDoctor
                            ? "You cancelled an appointment."
                            : "A patient has cancelled an appointment.",

                        Type =
                            NotificationType
                                .AppointmentCancelled
                                .ToString(),

                        
                    });
            }
        }

        private async Task SendCompletedNotificationAsync(Appointment appointment)
        {
            var patient = await _unitOfWork
                .Repository<Patient>()
                .GetByIdAsync(
                    appointment.PatientId);

            if (patient is null)
            {
                return;
            }

            await _notificationService.CreateAsync(
                new CreateNotificationDto
                {
                    UserId = patient.UserId,
                    AppointmentId = appointment.Id,

                    Title =
                        "Appointment Completed",

                    Message =
                        "Your appointment has been completed.",

                    Type =
                        NotificationType
                            .AppointmentCompleted
                            .ToString(),

                    
                });
        }

        private async Task SendNoShowNotificationAsync(Appointment appointment)
        {
            var patient = await _unitOfWork
                .Repository<Patient>()
                .GetByIdAsync(
                    appointment.PatientId);

            if (patient is null)
            {
                return;
            }

            await _notificationService.CreateAsync(
                new CreateNotificationDto
                {
                    UserId = patient.UserId,
                    AppointmentId = appointment.Id,

                    Title =
                        "Appointment Marked as No-Show",

                    Message =
                        "Your appointment has been marked as no-show because you did not attend.",

                    Type =
                        NotificationType
                            .AppointmentNoShow
                            .ToString(),

                    
                });
        }

       
        private async Task TrySendNotificationsAsync(Func<Task> notificationAction,Guid appointmentId,string operationName)
        {
            try
            {
                await notificationAction();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Appointment {AppointmentId} was saved successfully, but {OperationName} notifications failed.",
                    appointmentId,
                    operationName);
            }
        }
    }
}