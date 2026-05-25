using AutoMapper;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.AvailabilitySlots;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Implementation_Of_Services
{
    public class AvailabilitySlotService : IAvailabilitySlotService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateAvailabilitySlotDto> _createValidator;
        private readonly IValidator<UpdateAvailabilitySlotDto> _updateValidator;
        private readonly IValidator<AvailableSlotSearchDto> _searchValidator;

        public AvailabilitySlotService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateAvailabilitySlotDto> createValidator,
            IValidator<UpdateAvailabilitySlotDto> updateValidator,
            IValidator<AvailableSlotSearchDto> searchValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _searchValidator = searchValidator;
        }

        public async Task<ApiResponse<IReadOnlyList<AvailabilitySlotDto>>> GetDoctorAvailableSlotsAsync(Guid doctorId)
        {
            var doctorExists = await _unitOfWork
                .Repository<Doctor>()
                .AnyAsync(d => d.Id == doctorId && !d.IsDeleted);

            if (!doctorExists)
                throw new NotFoundException("Doctor not found.");

            var slots = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .GetAllAsync(s =>
                    s.DoctorId == doctorId &&
                    s.IsAvailable &&
                    !s.IsDeleted &&
                    s.StartTime > DateTime.UtcNow);

            var orderedSlots = slots
                .OrderBy(s => s.StartTime)
                .ToList();

            var result = _mapper.Map<IReadOnlyList<AvailabilitySlotDto>>(orderedSlots);

            return ApiResponse<IReadOnlyList<AvailabilitySlotDto>>.Ok(result);
        }

        public async Task<ApiResponse<IReadOnlyList<AvailabilitySlotDto>>> GetClinicAvailableSlotsAsync(Guid clinicId)
        {
            var clinicExists = await _unitOfWork
                .Repository<Clinic>()
                .AnyAsync(c => c.Id == clinicId && !c.IsDeleted);

            if (!clinicExists)
                throw new NotFoundException("Clinic not found.");

            var slots = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .GetAllAsync(s =>
                    s.ClinicId == clinicId &&
                    s.IsAvailable &&
                    !s.IsDeleted &&
                    s.StartTime > DateTime.UtcNow);

            var orderedSlots = slots
                .OrderBy(s => s.StartTime)
                .ToList();

            var result = _mapper.Map<IReadOnlyList<AvailabilitySlotDto>>(orderedSlots);

            return ApiResponse<IReadOnlyList<AvailabilitySlotDto>>.Ok(result);
        }

        public async Task<ApiResponse<AvailabilitySlotDto>> GetByIdAsync(Guid slotId)
        {
            var slot = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .FirstOrDefaultAsync(s => s.Id == slotId && !s.IsDeleted);

            if (slot is null)
                throw new NotFoundException("Availability slot not found.");

            var result = _mapper.Map<AvailabilitySlotDto>(slot);

            return ApiResponse<AvailabilitySlotDto>.Ok(result);
        }

        public async Task<ApiResponse<IReadOnlyList<AvailabilitySlotDto>>> SearchAvailableSlotsAsync(AvailableSlotSearchDto dto)
        {
            var validationResult = await _searchValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var slots = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .GetAllAsync(s =>
                    s.IsAvailable &&
                    !s.IsDeleted &&
                    s.StartTime > DateTime.UtcNow);

            var filteredSlots = slots.ToList();

            if (dto.DoctorId.HasValue)
            {
                filteredSlots = filteredSlots
                    .Where(s => s.DoctorId == dto.DoctorId.Value)
                    .ToList();
            }

            if (dto.ClinicId.HasValue)
            {
                filteredSlots = filteredSlots
                    .Where(s => s.ClinicId == dto.ClinicId.Value)
                    .ToList();
            }

            if (dto.SpecialtyId.HasValue)
            {
                var doctors = await _unitOfWork
                    .Repository<Doctor>()
                    .GetAllAsync(d =>
                        d.SpecialtyId == dto.SpecialtyId.Value &&
                        d.IsVerified &&
                        !d.IsDeleted);

                var doctorIds = doctors
                    .Select(d => d.Id)
                    .ToHashSet();

                filteredSlots = filteredSlots
                    .Where(s => doctorIds.Contains(s.DoctorId))
                    .ToList();
            }

            if (dto.Date.HasValue)
            {
                var date = dto.Date.Value.Date;

                filteredSlots = filteredSlots
                    .Where(s => s.StartTime.Date == date)
                    .ToList();
            }

            var orderedSlots = filteredSlots
                .OrderBy(s => s.StartTime)
                .ToList();

            var result = _mapper.Map<IReadOnlyList<AvailabilitySlotDto>>(orderedSlots);

            return ApiResponse<IReadOnlyList<AvailabilitySlotDto>>.Ok(result);
        }

        public async Task<ApiResponse<AvailabilitySlotDto>> CreateAsync(Guid doctorId, CreateAvailabilitySlotDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(d => d.Id == doctorId && !d.IsDeleted);

            if (doctor is null)
                throw new NotFoundException("Doctor not found.");

            if (!doctor.IsVerified)
                throw new ForbiddenException("Doctor must be verified before creating slots.");

            var clinic = await _unitOfWork
                .Repository<Clinic>()
                .FirstOrDefaultAsync(c => c.Id == dto.ClinicId && !c.IsDeleted);

            if (clinic is null)
                throw new NotFoundException("Clinic not found.");

            if (clinic.DoctorId != doctorId)
                throw new ForbiddenException("Clinic does not belong to this doctor.");

            var overlap = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .AnyAsync(s =>
                    s.DoctorId == doctorId &&
                    s.ClinicId == dto.ClinicId &&
                    !s.IsDeleted &&
                    dto.StartTime < s.EndTime &&
                    dto.EndTime > s.StartTime);

            if (overlap)
                throw new ConflictException("This time slot overlaps with an existing slot.");

            var slot = _mapper.Map<AvailabilitySlot>(dto);

            slot.DoctorId = doctorId;
            slot.ClinicId = dto.ClinicId;
            slot.IsAvailable = true;

            await _unitOfWork
                .Repository<AvailabilitySlot>()
                .AddAsync(slot);

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<AvailabilitySlotDto>(slot);

            return ApiResponse<AvailabilitySlotDto>.Ok(result, "Availability slot created successfully.");
        }

        public async Task<ApiResponse<AvailabilitySlotDto>> UpdateAsync(Guid doctorId, Guid slotId, UpdateAvailabilitySlotDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var slot = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .FirstOrDefaultAsync(s => s.Id == slotId && !s.IsDeleted);

            if (slot is null)
                throw new NotFoundException("Availability slot not found.");

            if (slot.DoctorId != doctorId)
                throw new ForbiddenException("You are not allowed to update this slot.");

            var hasActiveAppointment = await _unitOfWork
                .Repository<Appointment>()
                .AnyAsync(a =>
                    a.AvailabilitySlotId == slotId &&
                    a.Status != AppointmentStatus.Cancelled &&
                    !a.IsDeleted);

            if (hasActiveAppointment)
                throw new ConflictException("Cannot update a slot that already has an active appointment.");

            var overlap = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .AnyAsync(s =>
                    s.Id != slotId &&
                    s.DoctorId == doctorId &&
                    s.ClinicId == slot.ClinicId &&
                    !s.IsDeleted &&
                    dto.StartTime < s.EndTime &&
                    dto.EndTime > s.StartTime);

            if (overlap)
                throw new ConflictException("This time slot overlaps with an existing slot.");

            _mapper.Map(dto, slot);

            _unitOfWork
                .Repository<AvailabilitySlot>()
                .Update(slot);

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<AvailabilitySlotDto>(slot);

            return ApiResponse<AvailabilitySlotDto>.Ok(result, "Availability slot updated successfully.");
        }

        public async Task<ApiResponse> DeleteAsync(Guid doctorId, Guid slotId)
        {
            var slot = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .FirstOrDefaultAsync(s => s.Id == slotId && !s.IsDeleted);

            if (slot is null)
                throw new NotFoundException("Availability slot not found.");

            if (slot.DoctorId != doctorId)
                throw new ForbiddenException("You are not allowed to delete this slot.");

            var hasActiveAppointment = await _unitOfWork
                .Repository<Appointment>()
                .AnyAsync(a =>
                    a.AvailabilitySlotId == slotId &&
                    a.Status != AppointmentStatus.Cancelled &&
                    !a.IsDeleted);

            if (hasActiveAppointment)
                throw new ConflictException("Cannot delete a slot that already has an active appointment.");

            _unitOfWork
                .Repository<AvailabilitySlot>()
                .SoftDelete(slot);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Availability slot deleted successfully.");
        }

        public async Task<ApiResponse> MarkAsAvailableAsync(Guid doctorId, Guid slotId)
        {
            var slot = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .FirstOrDefaultAsync(s => s.Id == slotId && !s.IsDeleted);

            if (slot is null)
                throw new NotFoundException("Availability slot not found.");

            if (slot.DoctorId != doctorId)
                throw new ForbiddenException("You are not allowed to update this slot.");

            if (slot.StartTime <= DateTime.UtcNow)
                throw new BadRequestException("Cannot mark an expired slot as available.");

            var hasActiveAppointment = await _unitOfWork
                .Repository<Appointment>()
                .AnyAsync(a =>
                    a.AvailabilitySlotId == slotId &&
                    a.Status != AppointmentStatus.Cancelled &&
                    !a.IsDeleted);

            if (hasActiveAppointment)
                throw new ConflictException("Cannot mark a booked slot as available.");

            slot.IsAvailable = true;

            _unitOfWork
                .Repository<AvailabilitySlot>()
                .Update(slot);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Slot marked as available.");
        }

        public async Task<ApiResponse> MarkAsUnavailableAsync(Guid doctorId, Guid slotId)
        {
            var slot = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .FirstOrDefaultAsync(s => s.Id == slotId && !s.IsDeleted);

            if (slot is null)
                throw new NotFoundException("Availability slot not found.");

            if (slot.DoctorId != doctorId)
                throw new ForbiddenException("You are not allowed to update this slot.");

            var hasActiveAppointment = await _unitOfWork
                .Repository<Appointment>()
                .AnyAsync(a =>
                    a.AvailabilitySlotId == slotId &&
                    a.Status != AppointmentStatus.Cancelled &&
                    !a.IsDeleted);

            if (hasActiveAppointment)
                throw new ConflictException("Cannot mark a booked slot as unavailable.");

            slot.IsAvailable = false;

            _unitOfWork
                .Repository<AvailabilitySlot>()
                .Update(slot);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Slot marked as unavailable.");
        }
    }
}
