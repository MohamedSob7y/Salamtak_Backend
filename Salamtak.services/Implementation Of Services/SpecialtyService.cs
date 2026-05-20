using AutoMapper;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.Specialties;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Implementation_Of_Services
{
    public class SpecialtyService : ISpecialtyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateSpecialtyDto> _createValidator;
        private readonly IValidator<UpdateSpecialtyDto> _updateValidator;

        public SpecialtyService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateSpecialtyDto> createValidator,
            IValidator<UpdateSpecialtyDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<ApiResponse<IReadOnlyList<SpecialtyDto>>> GetAllAsync()
        {
            var specialties = await _unitOfWork
                .Repository<Specialty>()
                .GetAllAsync(s => !s.IsDeleted);

            var orderedSpecialties = specialties
                .OrderBy(s => s.Name)
                .ToList();

            var result = _mapper.Map<IReadOnlyList<SpecialtyDto>>(orderedSpecialties);

            return ApiResponse<IReadOnlyList<SpecialtyDto>>.Ok(result);
        }

        public async Task<ApiResponse<SpecialtyDto>> GetByIdAsync(Guid specialtyId)
        {
            var specialty = await _unitOfWork
                .Repository<Specialty>()
                .FirstOrDefaultAsync(s => s.Id == specialtyId && !s.IsDeleted);

            if (specialty is null)
                throw new NotFoundException("Specialty not found.");

            var result = _mapper.Map<SpecialtyDto>(specialty);

            return ApiResponse<SpecialtyDto>.Ok(result);
        }

        public async Task<ApiResponse<SpecialtyDto>> CreateAsync(CreateSpecialtyDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var name = dto.Name.Trim();
            var normalizedName = name.ToLower();

            var exists = await _unitOfWork
                .Repository<Specialty>()
                .AnyAsync(s =>
                    !s.IsDeleted &&
                    s.Name.ToLower() == normalizedName);

            if (exists)
                throw new ConflictException("Specialty already exists.");

            var specialty = new Specialty
            {
                Name = name,
                Description = dto.Description?.Trim()
            };

            await _unitOfWork
                .Repository<Specialty>()
                .AddAsync(specialty);

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<SpecialtyDto>(specialty);

            return ApiResponse<SpecialtyDto>.Ok(result, "Specialty created successfully.");
        }

        public async Task<ApiResponse<SpecialtyDto>> UpdateAsync(UpdateSpecialtyDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var specialty = await _unitOfWork
                .Repository<Specialty>()
                .FirstOrDefaultAsync(s => s.Id == dto.SpecialtyId && !s.IsDeleted);

            if (specialty is null)
                throw new NotFoundException("Specialty not found.");

            var name = dto.Name.Trim();
            var normalizedName = name.ToLower();

            var duplicateName = await _unitOfWork
                .Repository<Specialty>()
                .AnyAsync(s =>
                    !s.IsDeleted &&
                    s.Id != dto.SpecialtyId &&
                    s.Name.ToLower() == normalizedName);

            if (duplicateName)
                throw new ConflictException("Another specialty with the same name already exists.");

            specialty.Name = name;
            specialty.Description = dto.Description?.Trim();

            _unitOfWork
                .Repository<Specialty>()
                .Update(specialty);

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<SpecialtyDto>(specialty);

            return ApiResponse<SpecialtyDto>.Ok(result, "Specialty updated successfully.");
        }

        public async Task<ApiResponse> DeleteAsync(Guid specialtyId)
        {
            var specialty = await _unitOfWork
                .Repository<Specialty>()
                .FirstOrDefaultAsync(s => s.Id == specialtyId && !s.IsDeleted);

            if (specialty is null)
                throw new NotFoundException("Specialty not found.");

            var hasDoctors = await _unitOfWork
                .Repository<Doctor>()
                .AnyAsync(d =>
                    !d.IsDeleted &&
                    d.SpecialtyId == specialtyId);

            if (hasDoctors)
                throw new ConflictException("Cannot delete specialty because it is assigned to doctors.");

            _unitOfWork
                .Repository<Specialty>()
                .SoftDelete(specialty);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Specialty deleted successfully.");
        }
    }
}
