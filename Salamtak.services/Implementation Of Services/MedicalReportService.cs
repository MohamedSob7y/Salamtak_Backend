using AutoMapper;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.MedicalReports;
using Salamtak.Shared.Responses;

namespace Salamtak.services.Implementation_Of_Services
{
    public class MedicalReportService : IMedicalReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateMedicalReportEntryDto> _createValidator;
        private readonly IValidator<UpdateMedicalReportEntryDto> _updateValidator;

        public MedicalReportService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateMedicalReportEntryDto> createValidator,
            IValidator<UpdateMedicalReportEntryDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<ApiResponse<MedicalReportDto>> GetPatientReportAsync(Guid patientId)
        {
            var patientExists = await _unitOfWork.Repository<Patient>().AnyAsync(p => p.Id == patientId);
            if (!patientExists)
                throw new NotFoundException("Patient not found.");

            var report = await _unitOfWork.Repository<MedicalReport>().FirstOrDefaultAsync(r => r.PatientId == patientId);
            if (report is null)
                throw new NotFoundException("Medical report not found.");

            var result = _mapper.Map<MedicalReportDto>(report);
            return ApiResponse<MedicalReportDto>.Ok(result);
        }

        public async Task<ApiResponse<MedicalReportDto>> GetPatientReportForDoctorAsync(Guid doctorId, Guid patientId, Guid appointmentId)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(appointmentId);
            if (appointment is null)
                throw new NotFoundException("Appointment not found.");

            if (appointment.DoctorId != doctorId || appointment.PatientId != patientId)
                throw new ForbiddenException("Doctor is not allowed to access this medical report.");

            return await GetPatientReportAsync(patientId);
        }

        public async Task<ApiResponse<MedicalReportEntryDto>> AddEntryAsync(Guid doctorId, CreateMedicalReportEntryDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(dto.AppointmentId);
            if (appointment is null)
                throw new NotFoundException("Appointment not found.");

            if (appointment.DoctorId != doctorId)
                throw new ForbiddenException("You are not allowed to add a report entry for this appointment.");

            if (appointment.Status != AppointmentStatus.Completed)
                throw new BadRequestException("Medical reports can only be added after completing the appointment.");

            var report = await _unitOfWork.Repository<MedicalReport>().FirstOrDefaultAsync(r => r.PatientId == appointment.PatientId);

            if (report is null)
            {
                report = new MedicalReport { PatientId = appointment.PatientId };
                await _unitOfWork.Repository<MedicalReport>().AddAsync(report);
                await _unitOfWork.SaveChangesAsync();
            }

            var entry = new MedicalReportEntry
            {
                MedicalReportId = report.Id,
                AppointmentId = appointment.Id,
                DoctorId = doctorId,
                Diagnosis = dto.Diagnosis?.Trim(),
                Recommendations = dto.Recommendations?.Trim(),
                Notes = dto.Notes?.Trim()
            };

            await _unitOfWork.Repository<MedicalReportEntry>().AddAsync(entry);

            foreach (var prescriptionDto in dto.Prescriptions)
            {
                var prescription = new Prescription
                {
                    MedicalReportEntryId = entry.Id,
                    DrugName = prescriptionDto.DrugName.Trim(),
                    Dose = prescriptionDto.Dose?.Trim(),
                    Duration = prescriptionDto.Duration?.Trim(),
                    Instructions = prescriptionDto.Instructions?.Trim()
                };

                await _unitOfWork.Repository<Prescription>().AddAsync(prescription);
            }

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<MedicalReportEntryDto>(entry);
            return ApiResponse<MedicalReportEntryDto>.Ok(result, "Medical report entry added successfully.");
        }

        public async Task<ApiResponse<MedicalReportEntryDto>> UpdateEntryAsync(Guid doctorId, UpdateMedicalReportEntryDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var entry = await _unitOfWork.Repository<MedicalReportEntry>().GetByIdAsync(dto.EntryId);
            if (entry is null)
                throw new NotFoundException("Medical report entry not found.");

            if (entry.DoctorId != doctorId)
                throw new ForbiddenException("You are not allowed to update this report entry.");

            entry.Diagnosis = dto.Diagnosis?.Trim();
            entry.Recommendations = dto.Recommendations?.Trim();
            entry.Notes = dto.Notes?.Trim();

            _unitOfWork.Repository<MedicalReportEntry>().Update(entry);

            foreach (var prescriptionDto in dto.Prescriptions)
            {
                var prescription = await _unitOfWork.Repository<Prescription>().GetByIdAsync(prescriptionDto.PrescriptionId);

                if (prescription is null)
                    continue;

                if (prescription.MedicalReportEntryId != entry.Id)
                    throw new BadRequestException("Prescription does not belong to this report entry.");

                prescription.DrugName = prescriptionDto.DrugName.Trim();
                prescription.Dose = prescriptionDto.Dose?.Trim();
                prescription.Duration = prescriptionDto.Duration?.Trim();
                prescription.Instructions = prescriptionDto.Instructions?.Trim();

                _unitOfWork.Repository<Prescription>().Update(prescription);
            }

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<MedicalReportEntryDto>(entry);
            return ApiResponse<MedicalReportEntryDto>.Ok(result, "Medical report entry updated successfully.");
        }
    }
}
