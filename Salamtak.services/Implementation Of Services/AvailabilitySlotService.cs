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

        public AvailabilitySlotService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateAvailabilitySlotDto> createValidator,
            IValidator<UpdateAvailabilitySlotDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<ApiResponse<IReadOnlyList<AvailabilitySlotDto>>> GetDoctorAvailableSlotsAsync(Guid doctorId)
        {
            var slots = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .GetAllAsync(s => s.DoctorId == doctorId && s.IsAvailable);

            var result = _mapper.Map<IReadOnlyList<AvailabilitySlotDto>>(slots);

            return ApiResponse<IReadOnlyList<AvailabilitySlotDto>>.Ok(result);
        }

        public async Task<ApiResponse<IReadOnlyList<AvailabilitySlotDto>>> GetClinicAvailableSlotsAsync(Guid clinicId)
        {
            var slots = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .GetAllAsync(s => s.ClinicId == clinicId && s.IsAvailable);

            var result = _mapper.Map<IReadOnlyList<AvailabilitySlotDto>>(slots);

            return ApiResponse<IReadOnlyList<AvailabilitySlotDto>>.Ok(result);
        }

        public async Task<ApiResponse<AvailabilitySlotDto>> GetByIdAsync(Guid slotId)
        {
            var slot = await _unitOfWork.Repository<AvailabilitySlot>().GetByIdAsync(slotId);

            if (slot is null)
                throw new NotFoundException("Availability slot not found.");

            var result = _mapper.Map<AvailabilitySlotDto>(slot);

            return ApiResponse<AvailabilitySlotDto>.Ok(result);
        }

        public async Task<ApiResponse<AvailabilitySlotDto>> CreateAsync(CreateAvailabilitySlotDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));


            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(dto.DoctorId);

            if (doctor is null)
                throw new NotFoundException("Doctor not found.");

            if (!doctor.IsVerified)
                throw new ForbiddenException("Doctor must be verified before creating slots.");

            var clinic = await _unitOfWork.Repository<Clinic>().GetByIdAsync(dto.ClinicId);

            if (clinic is null)
                throw new NotFoundException("Clinic not found.");

            if (clinic.DoctorId != dto.DoctorId)
                throw new BadRequestException("Clinic does not belong to this doctor.");

            var overlap = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .AnyAsync(s =>
                    s.DoctorId == dto.DoctorId &&
                    s.ClinicId == dto.ClinicId &&
                    dto.StartTime < s.EndTime &&
                    dto.EndTime > s.StartTime);

            if (overlap)
                throw new ConflictException("This time slot overlaps with an existing slot.");

            var slot = new AvailabilitySlot
            {
                DoctorId = dto.DoctorId,
                ClinicId = dto.ClinicId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsAvailable = true
            };

            await _unitOfWork.Repository<AvailabilitySlot>().AddAsync(slot);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<AvailabilitySlotDto>(slot);

            return ApiResponse<AvailabilitySlotDto>.Ok(result, "Availability slot created successfully.");
        }

        public async Task<ApiResponse<AvailabilitySlotDto>> UpdateAsync(UpdateAvailabilitySlotDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));


            var slot = await _unitOfWork.Repository<AvailabilitySlot>().GetByIdAsync(dto.SlotId);

            if (slot is null)
                throw new NotFoundException("Availability slot not found.");

            var hasAppointment = await _unitOfWork
                 .Repository<Appointment>()
                 .AnyAsync(a =>
                  a.AvailabilitySlotId == dto.SlotId &&
                  a.Status != AppointmentStatus.Cancelled);

            if (hasAppointment)
                throw new ConflictException("Cannot update a slot that already has an Active appointment.");

            var overlap = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .AnyAsync(s =>
                    s.Id != dto.SlotId &&
                    s.DoctorId == slot.DoctorId &&
                    s.ClinicId == slot.ClinicId &&
                    dto.StartTime < s.EndTime &&
                    dto.EndTime > s.StartTime);

            if (overlap)
                throw new ConflictException("This time slot overlaps with an existing slot.");

            slot.StartTime = dto.StartTime;
            slot.EndTime = dto.EndTime;
            slot.IsAvailable = dto.IsAvailable;

            _unitOfWork.Repository<AvailabilitySlot>().Update(slot);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<AvailabilitySlotDto>(slot);

            return ApiResponse<AvailabilitySlotDto>.Ok(result, "Availability slot updated successfully.");
        }

        public async Task<ApiResponse> DeleteAsync(Guid slotId)
        {
            var slot = await _unitOfWork.Repository<AvailabilitySlot>().GetByIdAsync(slotId);

            if (slot is null)
                throw new NotFoundException("Availability slot not found.");

            var hasAppointment = await _unitOfWork
              .Repository<Appointment>()
              .AnyAsync(a =>
        a.AvailabilitySlotId == slotId &&
        a.Status != AppointmentStatus.Cancelled);

            if (hasAppointment)
                throw new ConflictException("Cannot delete a slot that already has an Active appointment.");

            _unitOfWork.Repository<AvailabilitySlot>().SoftDelete(slot);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Availability slot deleted successfully.");
        }

        public async Task<ApiResponse> MarkAsAvailableAsync(Guid slotId)
        {
            var slot = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .GetByIdAsync(slotId);

            if (slot is null)
                throw new NotFoundException("Availability slot not found.");

            var hasConfirmedAppointment = await _unitOfWork
                .Repository<Appointment>()
                .AnyAsync(a =>
                    a.AvailabilitySlotId == slotId &&
                    a.Status == AppointmentStatus.Confirmed);

            if (hasConfirmedAppointment)
                throw new ConflictException("Cannot mark a booked slot as available.");

            slot.IsAvailable = true;

            _unitOfWork.Repository<AvailabilitySlot>().Update(slot);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Slot marked as available.");
        }

        public async Task<ApiResponse> MarkAsUnavailableAsync(Guid slotId)
        {
            var slot = await _unitOfWork.Repository<AvailabilitySlot>().GetByIdAsync(slotId);

            if (slot is null)
                throw new NotFoundException("Availability slot not found.");

            slot.IsAvailable = false;

            _unitOfWork.Repository<AvailabilitySlot>().Update(slot);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Slot marked as unavailable.");
        }
    }
}
