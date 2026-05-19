using AutoMapper;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.Appointments;
using Salamtak.Shared.DTOs.MedicalReports;
using Salamtak.Shared.DTOs.Patients;
using Salamtak.Shared.Responses;

namespace Salamtak.services.Implementation_Of_Services
{
    public class PatientService : IPatientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<UpdatePatientProfileDto> _updateValidator;

        public PatientService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<UpdatePatientProfileDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _updateValidator = updateValidator;
        }

        public async Task<ApiResponse<PatientProfileDto>> GetProfileAsync(Guid patientId)
        {
            var patient = await _unitOfWork.Repository<Patient>().GetByIdAsync(patientId);
            if (patient is null)
                throw new NotFoundException("Patient not found.");

            var result = _mapper.Map<PatientProfileDto>(patient);
            return ApiResponse<PatientProfileDto>.Ok(result);
        }

        public async Task<ApiResponse<PatientProfileDto>> UpdateProfileAsync(Guid patientId, UpdatePatientProfileDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var patient = await _unitOfWork.Repository<Patient>().GetByIdAsync(patientId);

            if (patient is null)
                throw new NotFoundException("Patient not found.");

            if (!Enum.TryParse<Gender>(dto.Gender, true, out var gender))
                throw new BadRequestException("Invalid gender.");

            patient.DateOfBirth = dto.DateOfBirth;
            patient.Gender = gender;
            patient.Address = dto.Address?.Trim();
            patient.BloodType = dto.BloodType?.Trim();
            patient.Height = dto.Height;
            patient.Weight = dto.Weight;

            _unitOfWork.Repository<Patient>().Update(patient);

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<PatientProfileDto>(patient);

            return ApiResponse<PatientProfileDto>.Ok(result, "Patient profile updated successfully.");
        }

        public async Task<ApiResponse<IReadOnlyList<PatientAppointmentDto>>> GetAppointmentsAsync(Guid patientId)
        {
            var patientExists = await _unitOfWork.Repository<Patient>().AnyAsync(p => p.Id == patientId);
            if (!patientExists)
                throw new NotFoundException("Patient not found.");

            var appointments = await _unitOfWork.Repository<Appointment>().GetAllAsync(a => a.PatientId == patientId);
            var result = _mapper.Map<IReadOnlyList<PatientAppointmentDto>>(appointments);

            return ApiResponse<IReadOnlyList<PatientAppointmentDto>>.Ok(result);
        }

        public async Task<ApiResponse<MedicalReportDto>> GetMedicalHistoryAsync(Guid patientId)
        {
            var patientExists = await _unitOfWork
                .Repository<Patient>()
                .AnyAsync(p => p.Id == patientId);

            if (!patientExists)
                throw new NotFoundException("Patient not found.");

            var report = await _unitOfWork
                .Repository<MedicalReport>()
                .FirstOrDefaultAsync(r => r.PatientId == patientId);

            if (report is null)
                throw new NotFoundException("Medical history not found.");

            var entries = await _unitOfWork
                .Repository<MedicalReportEntry>()
                .GetAllAsync(e => e.MedicalReportId == report.Id);

            foreach (var entry in entries)
            {
                var prescriptions = await _unitOfWork
                    .Repository<Prescription>()
                    .GetAllAsync(p => p.MedicalReportEntryId == entry.Id);

                entry.Prescriptions = prescriptions.ToList();
            }

            report.Entries = entries.ToList();

            var result = _mapper.Map<MedicalReportDto>(report);

            return ApiResponse<MedicalReportDto>.Ok(result);
        }
    }
}
