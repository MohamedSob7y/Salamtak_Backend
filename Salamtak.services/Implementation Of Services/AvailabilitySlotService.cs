    using AutoMapper;
    using FluentValidation;
    using Salamtak.Domain.Interfaces.UnitOfWork;
    using Salamtak.Domain.Models;
    using Salamtak.Domain.Models.Enums;
    using Salamtak.services.Abstractions.Interfaces_Services;
    using Salamtak.services.Exceptions;
    using Salamtak.Shared.DTOs.AvailabilitySlots;
    using Salamtak.Shared.Responses;

    namespace Salamtak.services.Implementation_Of_Services
    {
        public class AvailabilitySlotService
            : IAvailabilitySlotService
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;

            private readonly
                IValidator<CreateAvailabilitySlotDto>
                _createValidator;

            private readonly
                IValidator<UpdateAvailabilitySlotDto>
                _updateValidator;

            private readonly
                IValidator<AvailableSlotSearchDto>
                _searchValidator;

            public AvailabilitySlotService(
                IUnitOfWork unitOfWork,
                IMapper mapper,
                IValidator<CreateAvailabilitySlotDto>
                    createValidator,
                IValidator<UpdateAvailabilitySlotDto>
                    updateValidator,
                IValidator<AvailableSlotSearchDto>
                    searchValidator)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _createValidator = createValidator;
                _updateValidator = updateValidator;
                _searchValidator = searchValidator;
            }

            public async Task<
                ApiResponse<IReadOnlyList<AvailabilitySlotDto>>>
                GetDoctorAvailableSlotsAsync(
                    Guid doctorId)
            {
                var doctorExists = await _unitOfWork
                    .Repository<Doctor>()
                    .AnyAsync(doctor =>
                        doctor.Id == doctorId);

                if (!doctorExists)
                {
                    throw new NotFoundException(
                        "Doctor not found.");
                }

                var slots = await _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .GetAllAsync(slot =>
                        slot.DoctorId == doctorId &&
                        slot.IsAvailable &&
                        slot.StartTime > DateTime.UtcNow);

                var orderedSlots = slots
                    .OrderBy(slot => slot.StartTime)
                    .ToList();

                var result = _mapper.Map<
                    IReadOnlyList<AvailabilitySlotDto>>(
                        orderedSlots);

                return ApiResponse<
                    IReadOnlyList<AvailabilitySlotDto>>.Ok(
                        result,
                        "Doctor available slots retrieved successfully.");
            }

            public async Task<
                ApiResponse<IReadOnlyList<AvailabilitySlotDto>>>
                GetClinicAvailableSlotsAsync(
                    Guid clinicId)
            {
                var clinicExists = await _unitOfWork
                    .Repository<Clinic>()
                    .AnyAsync(clinic =>
                        clinic.Id == clinicId);

                if (!clinicExists)
                {
                    throw new NotFoundException(
                        "Clinic not found.");
                }

                var slots = await _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .GetAllAsync(slot =>
                        slot.ClinicId == clinicId &&
                        slot.IsAvailable &&
                        slot.StartTime > DateTime.UtcNow);

                var orderedSlots = slots
                    .OrderBy(slot => slot.StartTime)
                    .ToList();

                var result = _mapper.Map<
                    IReadOnlyList<AvailabilitySlotDto>>(
                        orderedSlots);

                return ApiResponse<
                    IReadOnlyList<AvailabilitySlotDto>>.Ok(
                        result,
                        "Clinic available slots retrieved successfully.");
            }

            public async Task<ApiResponse<AvailabilitySlotDto>>
                GetByIdAsync(
                    Guid slotId)
            {
                var slot = await _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .GetByIdAsync(slotId);

                if (slot is null)
                {
                    throw new NotFoundException(
                        "Availability slot not found.");
                }

                var result =
                    _mapper.Map<AvailabilitySlotDto>(
                        slot);

                return ApiResponse<AvailabilitySlotDto>.Ok(
                    result,
                    "Availability slot retrieved successfully.");
            }

            public async Task<
                ApiResponse<IReadOnlyList<AvailabilitySlotDto>>>
                SearchAvailableSlotsAsync(
                    AvailableSlotSearchDto dto)
            {
                var validationResult =
                    await _searchValidator.ValidateAsync(dto);

                if (!validationResult.IsValid)
                {
                    throw new AppValidationException(
                        validationResult.Errors
                            .Select(error =>
                                error.ErrorMessage));
                }

                if (dto.DoctorId.HasValue)
                {
                    var doctorExists = await _unitOfWork
                        .Repository<Doctor>()
                        .AnyAsync(doctor =>
                            doctor.Id ==
                            dto.DoctorId.Value);

                    if (!doctorExists)
                    {
                        throw new NotFoundException(
                            "Doctor not found.");
                    }
                }

                if (dto.ClinicId.HasValue)
                {
                    var clinicExists = await _unitOfWork
                        .Repository<Clinic>()
                        .AnyAsync(clinic =>
                            clinic.Id ==
                            dto.ClinicId.Value);

                    if (!clinicExists)
                    {
                        throw new NotFoundException(
                            "Clinic not found.");
                    }
                }

                if (dto.SpecialtyId.HasValue)
                {
                    var specialtyExists =
                        await _unitOfWork
                            .Repository<Specialty>()
                            .AnyAsync(specialty =>
                                specialty.Id ==
                                dto.SpecialtyId.Value);

                    if (!specialtyExists)
                    {
                        throw new NotFoundException(
                            "Specialty not found.");
                    }
                }

                var slots = await _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .GetAllAsync(slot =>
                        slot.IsAvailable &&
                        slot.StartTime > DateTime.UtcNow);

                var filteredSlots =
                    slots.AsEnumerable();

                if (dto.DoctorId.HasValue)
                {
                    filteredSlots =
                        filteredSlots.Where(slot =>
                            slot.DoctorId ==
                            dto.DoctorId.Value);
                }

                if (dto.ClinicId.HasValue)
                {
                    filteredSlots =
                        filteredSlots.Where(slot =>
                            slot.ClinicId ==
                            dto.ClinicId.Value);
                }

                if (dto.SpecialtyId.HasValue)
                {
                    var doctors = await _unitOfWork
                        .Repository<Doctor>()
                        .GetAllAsync(doctor =>
                            doctor.SpecialtyId ==
                            dto.SpecialtyId.Value &&
                            doctor.IsVerified &&
                            doctor.VerificationStatus ==
                            DoctorVerificationStatus.Verified);

                    var doctorIds = doctors
                        .Select(doctor => doctor.Id)
                        .ToHashSet();

                    filteredSlots =
                        filteredSlots.Where(slot =>
                            doctorIds.Contains(
                                slot.DoctorId));
                }

                if (dto.Date.HasValue)
                {
                    var requestedDate =
                        dto.Date.Value.Date;

                    var nextDate =
                        requestedDate.AddDays(1);

                    filteredSlots =
                        filteredSlots.Where(slot =>
                            slot.StartTime >= requestedDate &&
                            slot.StartTime < nextDate);
                }

                var orderedSlots = filteredSlots
                    .OrderBy(slot => slot.StartTime)
                    .ToList();

                var result = _mapper.Map<
                    IReadOnlyList<AvailabilitySlotDto>>(
                        orderedSlots);

                return ApiResponse<
                    IReadOnlyList<AvailabilitySlotDto>>.Ok(
                        result,
                        "Available slots retrieved successfully.");
            }

            public async Task<ApiResponse<AvailabilitySlotDto>>
                CreateAsync(
                    Guid doctorUserId,
                    CreateAvailabilitySlotDto dto)
            {
                var validationResult =
                    await _createValidator.ValidateAsync(dto);

                if (!validationResult.IsValid)
                {
                    throw new AppValidationException(
                        validationResult.Errors
                            .Select(error =>
                                error.ErrorMessage));
                }

                var doctor =
                    await GetDoctorByUserIdAsync(
                        doctorUserId);

                EnsureDoctorIsVerified(doctor);

                var clinic = await _unitOfWork
                    .Repository<Clinic>()
                    .GetByIdAsync(dto.ClinicId);

                if (clinic is null)
                {
                    throw new NotFoundException(
                        "Clinic not found.");
                }

                /*
                 * العيادة ليست مملوكة لطبيب واحد.
                 * يجب فقط التأكد أن الطبيب مرتبط بها
                 * من خلال DoctorClinics.
                 */
                await EnsureDoctorClinicRelationAsync(
                    doctor.Id,
                    clinic.Id);

                /*
                 * التداخل يتم فحصه على مستوى الطبيب بالكامل،
                 * وليس داخل عيادة واحدة فقط.
                 *
                 * لأن الطبيب لا يمكنه العمل في عيادتين
                 * في نفس الوقت.
                 */
                var hasOverlap = await _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .AnyAsync(existingSlot =>
                        existingSlot.DoctorId ==
                        doctor.Id &&
                        dto.StartTime <
                        existingSlot.EndTime &&
                        dto.EndTime >
                        existingSlot.StartTime);

                if (hasOverlap)
                {
                    throw new ConflictException(
                        "This time slot overlaps with another slot for the same doctor.");
                }

                var slot = new AvailabilitySlot
                {
                    DoctorId = doctor.Id,
                    ClinicId = clinic.Id,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    IsAvailable = true
                };

                await _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .AddAsync(slot);

                await _unitOfWork.SaveChangesAsync();

                var result =
                    _mapper.Map<AvailabilitySlotDto>(
                        slot);

                return ApiResponse<AvailabilitySlotDto>.Ok(
                    result,
                    "Availability slot created successfully.");
            }

            public async Task<ApiResponse<AvailabilitySlotDto>>
                UpdateAsync(
                    Guid doctorUserId,
                    Guid slotId,
                    UpdateAvailabilitySlotDto dto)
            {
                var validationResult =
                    await _updateValidator.ValidateAsync(dto);

                if (!validationResult.IsValid)
                {
                    throw new AppValidationException(
                        validationResult.Errors
                            .Select(error =>
                                error.ErrorMessage));
                }

                var doctor =
                    await GetDoctorByUserIdAsync(
                        doctorUserId);

                var slot =
                    await GetSlotAsync(slotId);

                EnsureSlotOwnership(
                    slot,
                    doctor.Id);

                await EnsureDoctorClinicRelationAsync(
                    doctor.Id,
                    slot.ClinicId);

                var hasActiveAppointment =
                    await HasActiveAppointmentAsync(
                        slot.Id);

                if (hasActiveAppointment)
                {
                    throw new ConflictException(
                        "Cannot update a slot that has an active appointment.");
                }

                var hasOverlap = await _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .AnyAsync(existingSlot =>
                        existingSlot.Id != slot.Id &&
                        existingSlot.DoctorId ==
                        doctor.Id &&
                        dto.StartTime <
                        existingSlot.EndTime &&
                        dto.EndTime >
                        existingSlot.StartTime);

                if (hasOverlap)
                {
                    throw new ConflictException(
                        "This time slot overlaps with another slot for the same doctor.");
                }

                slot.StartTime =
                    dto.StartTime;

                slot.EndTime =
                    dto.EndTime;

                _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .Update(slot);

                await _unitOfWork.SaveChangesAsync();

                var result =
                    _mapper.Map<AvailabilitySlotDto>(
                        slot);

                return ApiResponse<AvailabilitySlotDto>.Ok(
                    result,
                    "Availability slot updated successfully.");
            }

            public async Task<ApiResponse>
                DeleteAsync(
                    Guid doctorUserId,
                    Guid slotId)
            {
                var doctor =
                    await GetDoctorByUserIdAsync(
                        doctorUserId);

                var slot =
                    await GetSlotAsync(slotId);

                EnsureSlotOwnership(
                    slot,
                    doctor.Id);

                var hasActiveAppointment =
                    await HasActiveAppointmentAsync(
                        slot.Id);

                if (hasActiveAppointment)
                {
                    throw new ConflictException(
                        "Cannot delete a slot that has an active appointment.");
                }

                _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .SoftDelete(slot);

                await _unitOfWork.SaveChangesAsync();

                return ApiResponse.Ok(
                    "Availability slot deleted successfully.");
            }

            public async Task<ApiResponse>
                MarkAsAvailableAsync(
                    Guid doctorUserId,
                    Guid slotId)
            {
                var doctor =
                    await GetDoctorByUserIdAsync(
                        doctorUserId);

                var slot =
                    await GetSlotAsync(slotId);

                EnsureSlotOwnership(
                    slot,
                    doctor.Id);

                await EnsureDoctorClinicRelationAsync(
                    doctor.Id,
                    slot.ClinicId);

                if (slot.StartTime <=
                    DateTime.UtcNow)
                {
                    throw new BadRequestException(
                        "Cannot mark an expired slot as available.");
                }

                var hasActiveAppointment =
                    await HasActiveAppointmentAsync(
                        slot.Id);

                if (hasActiveAppointment)
                {
                    throw new ConflictException(
                        "Cannot mark a booked slot as available.");
                }

                if (slot.IsAvailable)
                {
                    throw new ConflictException(
                        "Slot is already available.");
                }

                slot.IsAvailable = true;

                _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .Update(slot);

                await _unitOfWork.SaveChangesAsync();

                return ApiResponse.Ok(
                    "Slot marked as available.");
            }

            public async Task<ApiResponse>
                MarkAsUnavailableAsync(
                    Guid doctorUserId,
                    Guid slotId)
            {
                var doctor =
                    await GetDoctorByUserIdAsync(
                        doctorUserId);

                var slot =
                    await GetSlotAsync(slotId);

                EnsureSlotOwnership(
                    slot,
                    doctor.Id);

                var hasActiveAppointment =
                    await HasActiveAppointmentAsync(
                        slot.Id);

                if (hasActiveAppointment)
                {
                    throw new ConflictException(
                        "Cannot mark a slot with an active appointment as unavailable.");
                }

                if (!slot.IsAvailable)
                {
                    throw new ConflictException(
                        "Slot is already unavailable.");
                }

                slot.IsAvailable = false;

                _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .Update(slot);

                await _unitOfWork.SaveChangesAsync();

                return ApiResponse.Ok(
                    "Slot marked as unavailable.");
            }

            private async Task<Doctor>
                GetDoctorByUserIdAsync(
                    Guid doctorUserId)
            {
                var doctor = await _unitOfWork
                    .Repository<Doctor>()
                    .FirstOrDefaultAsync(item =>
                        item.UserId ==
                        doctorUserId);

                if (doctor is null)
                {
                    throw new NotFoundException(
                        "Doctor profile not found.");
                }

                return doctor;
            }

            private async Task<AvailabilitySlot>
                GetSlotAsync(
                    Guid slotId)
            {
                var slot = await _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .GetByIdAsync(slotId);

                if (slot is null)
                {
                    throw new NotFoundException(
                        "Availability slot not found.");
                }

                return slot;
            }

            private async Task
                EnsureDoctorClinicRelationAsync(
                    Guid doctorId,
                    Guid clinicId)
            {
                var relationExists =
                    await _unitOfWork
                        .Repository<DoctorClinic>()
                        .AnyAsync(relation =>
                            relation.DoctorId ==
                            doctorId &&
                            relation.ClinicId ==
                            clinicId);

                if (!relationExists)
                {
                    throw new ForbiddenException(
                        "Doctor is not registered at the selected clinic.");
                }
            }

            private async Task<bool>
                HasActiveAppointmentAsync(
                    Guid slotId)
            {
                return await _unitOfWork
                    .Repository<Appointment>()
                    .AnyAsync(appointment =>
                        appointment.AvailabilitySlotId ==
                        slotId &&
                        (
                            appointment.Status ==
                            AppointmentStatus.Pending ||
                            appointment.Status ==
                            AppointmentStatus.Confirmed
                        ));
            }

            private static void
                EnsureSlotOwnership(
                    AvailabilitySlot slot,
                    Guid doctorId)
            {
                if (slot.DoctorId != doctorId)
                {
                    throw new ForbiddenException(
                        "You are not allowed to manage this slot.");
                }
            }

            private static void
                EnsureDoctorIsVerified(
                    Doctor doctor)
            {
                if (!doctor.IsVerified ||
                    doctor.VerificationStatus !=
                    DoctorVerificationStatus.Verified)
                {
                    throw new ForbiddenException(
                        "Doctor must be verified before creating availability slots.");
                }
            }
        }
    }