using AutoMapper;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
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
                .AnyAsync(d => d.Id == doctorId);

            if (!doctorExists)
                throw new NotFoundException("Doctor not found.");

            var clinics = await _unitOfWork
                .Repository<Clinic>()
                .GetAllAsync(c => c.DoctorId == doctorId);

            var result = _mapper.Map<IReadOnlyList<ClinicDto>>(clinics);

            return ApiResponse<IReadOnlyList<ClinicDto>>.Ok(result);
        }

        public async Task<ApiResponse<ClinicDto>> GetByIdAsync(Guid clinicId)
        {
            var clinic = await _unitOfWork.Repository<Clinic>().GetByIdAsync(clinicId);

            if (clinic is null)
                throw new NotFoundException("Clinic not found.");

            var result = _mapper.Map<ClinicDto>(clinic);

            return ApiResponse<ClinicDto>.Ok(result);
        }

        public async Task<ApiResponse<ClinicDto>> CreateAsync(CreateClinicDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(dto.DoctorId);

            if (doctor is null)
                throw new NotFoundException("Doctor not found.");

            if (!doctor.IsVerified)
                throw new ForbiddenException("Doctor must be verified before adding clinics.");

            var duplicateClinic = await _unitOfWork
                .Repository<Clinic>()
                .AnyAsync(c =>
                    c.DoctorId == dto.DoctorId &&
                    c.Name.ToLower() == dto.Name.Trim().ToLower() &&
                    c.City.ToLower() == dto.City.Trim().ToLower());

            if (duplicateClinic)
                throw new ConflictException("Clinic already exists for this doctor in the same city.");

            var clinic = new Clinic
            {
                DoctorId = dto.DoctorId,
                Name = dto.Name.Trim(),
                Address = dto.Address.Trim(),
                City = dto.City.Trim(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };

            await _unitOfWork.Repository<Clinic>().AddAsync(clinic);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ClinicDto>(clinic);

            return ApiResponse<ClinicDto>.Ok(result, "Clinic created successfully.");
        }

        public async Task<ApiResponse<ClinicDto>> UpdateAsync(UpdateClinicDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));


            var clinic = await _unitOfWork.Repository<Clinic>().GetByIdAsync(dto.ClinicId);

            if (clinic is null)
                throw new NotFoundException("Clinic not found.");

            var duplicateClinic = await _unitOfWork
                .Repository<Clinic>()
                .AnyAsync(c =>
                    c.Id != dto.ClinicId &&
                    c.DoctorId == clinic.DoctorId &&
                    c.Name.ToLower() == dto.Name.Trim().ToLower() &&
                    c.City.ToLower() == dto.City.Trim().ToLower());

            if (duplicateClinic)
                throw new ConflictException("Another clinic with the same name already exists for this doctor.");

            clinic.Name = dto.Name.Trim();
            clinic.Address = dto.Address.Trim();
            clinic.City = dto.City.Trim();
            clinic.PhoneNumber = dto.PhoneNumber?.Trim();
            clinic.Latitude = dto.Latitude;
            clinic.Longitude = dto.Longitude;

            _unitOfWork.Repository<Clinic>().Update(clinic);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ClinicDto>(clinic);

            return ApiResponse<ClinicDto>.Ok(result, "Clinic updated successfully.");
        }

        public async Task<ApiResponse> DeleteAsync(Guid clinicId)
        {
            var clinic = await _unitOfWork.Repository<Clinic>().GetByIdAsync(clinicId);

            if (clinic is null)
                throw new NotFoundException("Clinic not found.");

            var hasAppointments = await _unitOfWork
                .Repository<Appointment>()
                .AnyAsync(a => a.ClinicId == clinicId);

            if (hasAppointments)
                throw new ConflictException("Cannot delete clinic because it has appointments.");

            _unitOfWork.Repository<Clinic>().SoftDelete(clinic);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Clinic deleted successfully.");
        }
    }
}
