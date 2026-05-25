using AutoMapper;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.Clinics;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Implementation_Of_Services
{
    public class ClinicService : IClinicService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateClinicDto> _createValidator;
        private readonly IValidator<UpdateClinicDto> _updateValidator;

        public ClinicService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateClinicDto> createValidator,
            IValidator<UpdateClinicDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<ApiResponse<IReadOnlyList<ClinicDto>>> GetDoctorClinicsAsync(Guid doctorId)
        {
            var doctorExists = await _unitOfWork
                .Repository<Doctor>()
                .AnyAsync(d => d.Id == doctorId && !d.IsDeleted);

            if (!doctorExists)
                throw new NotFoundException("Doctor not found.");

            var clinics = await _unitOfWork
                .Repository<Clinic>()
                .GetAllAsync(c => c.DoctorId == doctorId && !c.IsDeleted);

            var orderedClinics = clinics
                .OrderBy(c => c.Name)
                .ToList();

            var result = _mapper.Map<IReadOnlyList<ClinicDto>>(orderedClinics);

            return ApiResponse<IReadOnlyList<ClinicDto>>.Ok(result);
        }

        public async Task<ApiResponse<ClinicDto>> GetByIdAsync(Guid clinicId)
        {
            var clinic = await _unitOfWork
                .Repository<Clinic>()
                .FirstOrDefaultAsync(c => c.Id == clinicId && !c.IsDeleted);

            if (clinic is null)
                throw new NotFoundException("Clinic not found.");

            var result = _mapper.Map<ClinicDto>(clinic);

            return ApiResponse<ClinicDto>.Ok(result);
        }

        public async Task<ApiResponse<ClinicDto>> CreateAsync(Guid doctorId, CreateClinicDto dto)
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
                throw new ForbiddenException("Doctor must be verified before creating clinics.");

            var name = dto.Name.Trim();
            var normalizedName = name.ToLower();

            var duplicateClinic = await _unitOfWork
                .Repository<Clinic>()
                .AnyAsync(c =>
                    c.DoctorId == doctorId &&
                    !c.IsDeleted &&
                    c.Name.ToLower() == normalizedName);

            if (duplicateClinic)
                throw new ConflictException("Clinic with the same name already exists for this doctor.");

            var clinic = new Clinic
            {
                DoctorId = doctorId,
                Name = name,
                Address = dto.Address.Trim(),
                City = dto.City.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim()
            };

            await _unitOfWork
                .Repository<Clinic>()
                .AddAsync(clinic);

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ClinicDto>(clinic);

            return ApiResponse<ClinicDto>.Ok(result, "Clinic created successfully.");
        }

        public async Task<ApiResponse<ClinicDto>> UpdateAsync(Guid doctorId, Guid clinicId, UpdateClinicDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var clinic = await _unitOfWork
                .Repository<Clinic>()
                .FirstOrDefaultAsync(c => c.Id == clinicId && !c.IsDeleted);

            if (clinic is null)
                throw new NotFoundException("Clinic not found.");

            if (clinic.DoctorId != doctorId)
                throw new ForbiddenException("You are not allowed to update this clinic.");

            var name = dto.Name.Trim();
            var normalizedName = name.ToLower();

            var duplicateClinic = await _unitOfWork
                .Repository<Clinic>()
                .AnyAsync(c =>
                    c.DoctorId == doctorId &&
                    c.Id != clinicId &&
                    !c.IsDeleted &&
                    c.Name.ToLower() == normalizedName);

            if (duplicateClinic)
                throw new ConflictException("Another clinic with the same name already exists for this doctor.");

            clinic.Name = name;
            clinic.Address = dto.Address.Trim();
            clinic.City = dto.City.Trim();
            clinic.PhoneNumber = dto.PhoneNumber.Trim();

            _unitOfWork
                .Repository<Clinic>()
                .Update(clinic);

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ClinicDto>(clinic);

            return ApiResponse<ClinicDto>.Ok(result, "Clinic updated successfully.");
        }

        public async Task<ApiResponse> DeleteAsync(Guid doctorId, Guid clinicId)
        {
            var clinic = await _unitOfWork
                .Repository<Clinic>()
                .FirstOrDefaultAsync(c => c.Id == clinicId && !c.IsDeleted);

            if (clinic is null)
                throw new NotFoundException("Clinic not found.");

            if (clinic.DoctorId != doctorId)
                throw new ForbiddenException("You are not allowed to delete this clinic.");

            var hasActiveAppointments = await _unitOfWork
                .Repository<Appointment>()
                .AnyAsync(a =>
                    a.ClinicId == clinicId &&
                    a.Status != AppointmentStatus.Cancelled &&
                    !a.IsDeleted);

            if (hasActiveAppointments)
                throw new ConflictException("Cannot delete clinic because it has active appointments.");

            var hasFutureSlots = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .AnyAsync(s =>
                    s.ClinicId == clinicId &&
                    s.StartTime > DateTime.UtcNow &&
                    !s.IsDeleted);

            if (hasFutureSlots)
                throw new ConflictException("Cannot delete clinic because it has future availability slots.");

            _unitOfWork
                .Repository<Clinic>()
                .SoftDelete(clinic);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Clinic deleted successfully.");
        }
    }
}
