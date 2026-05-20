using AutoMapper;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.MedicalReports;
using Salamtak.Shared.DTOs.Prescriptions;
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
            var patient = await _unitOfWork
                .Repository<Patient>()
                .GetByIdAsync(patientId);

            if (patient is null)
                throw new NotFoundException("Patient not found.");

            var report = await _unitOfWork
                .Repository<MedicalReport>()
                .FirstOrDefaultAsync(r => r.PatientId == patientId);

            if (report is null)
                throw new NotFoundException("Medical report not found.");

            var result = await BuildMedicalReportDtoAsync(report, patient);

            return ApiResponse<MedicalReportDto>.Ok(result);
        }

        public async Task<ApiResponse<MedicalReportDto>> GetPatientReportForDoctorAsync(
            Guid doctorId,
            Guid patientId,
            Guid appointmentId)
        {
            var doctorExists = await _unitOfWork
                .Repository<Doctor>()
                .AnyAsync(d => d.Id == doctorId);

            if (!doctorExists)
                throw new NotFoundException("Doctor not found.");

            var patient = await _unitOfWork
                .Repository<Patient>()
                .GetByIdAsync(patientId);

            if (patient is null)
                throw new NotFoundException("Patient not found.");

            var appointment = await _unitOfWork
                .Repository<Appointment>()
                .GetByIdAsync(appointmentId);

            if (appointment is null)
                throw new NotFoundException("Appointment not found.");

            if (appointment.DoctorId != doctorId || appointment.PatientId != patientId)
                throw new ForbiddenException("Doctor is not allowed to access this medical report.");

            if (appointment.Status == AppointmentStatus.Cancelled)
                throw new ForbiddenException("Cancelled appointments do not allow access to medical reports.");

            var report = await _unitOfWork
                .Repository<MedicalReport>()
                .FirstOrDefaultAsync(r => r.PatientId == patientId);

            if (report is null)
                throw new NotFoundException("Medical report not found.");

            var result = await BuildMedicalReportDtoAsync(report, patient);

            return ApiResponse<MedicalReportDto>.Ok(result);
        }

        public async Task<ApiResponse<MedicalReportEntryDto>> AddEntryAsync(
            Guid doctorId,
            CreateMedicalReportEntryDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var doctorExists = await _unitOfWork
                .Repository<Doctor>()
                .AnyAsync(d => d.Id == doctorId);

            if (!doctorExists)
                throw new NotFoundException("Doctor not found.");

            var appointment = await _unitOfWork
                .Repository<Appointment>()
                .GetByIdAsync(dto.AppointmentId);

            if (appointment is null)
                throw new NotFoundException("Appointment not found.");

            if (appointment.DoctorId != doctorId)
                throw new ForbiddenException("You are not allowed to add a report entry for this appointment.");

            if (appointment.Status != AppointmentStatus.Completed)
                throw new BadRequestException("Medical reports can only be added after completing the appointment.");

            var entryAlreadyExists = await _unitOfWork
                .Repository<MedicalReportEntry>()
                .AnyAsync(e => e.AppointmentId == appointment.Id);

            if (entryAlreadyExists)
                throw new ConflictException("A medical report entry already exists for this appointment.");

            var report = await _unitOfWork
                .Repository<MedicalReport>()
                .FirstOrDefaultAsync(r => r.PatientId == appointment.PatientId);

            if (report is null)
            {
                report = new MedicalReport
                {
                    PatientId = appointment.PatientId
                };

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

            await _unitOfWork
                .Repository<MedicalReportEntry>()
                .AddAsync(entry);

            await _unitOfWork.SaveChangesAsync();

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

                await _unitOfWork
                    .Repository<Prescription>()
                    .AddAsync(prescription);
            }

            await _unitOfWork.SaveChangesAsync();

            var result = await BuildMedicalReportEntryDtoAsync(entry);

            return ApiResponse<MedicalReportEntryDto>.Ok(
                result,
                "Medical report entry added successfully.");
        }

        public async Task<ApiResponse<MedicalReportEntryDto>> UpdateEntryAsync(
            Guid doctorId,
            UpdateMedicalReportEntryDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var entry = await _unitOfWork
                .Repository<MedicalReportEntry>()
                .GetByIdAsync(dto.EntryId);

            if (entry is null)
                throw new NotFoundException("Medical report entry not found.");

            if (entry.DoctorId != doctorId)
                throw new ForbiddenException("You are not allowed to update this report entry.");

            var appointment = await _unitOfWork
                .Repository<Appointment>()
                .GetByIdAsync(entry.AppointmentId);

            if (appointment is null)
                throw new NotFoundException("Related appointment not found.");

            if (appointment.Status != AppointmentStatus.Completed)
                throw new BadRequestException("Only completed appointment report entries can be updated.");

            entry.Diagnosis = dto.Diagnosis?.Trim();
            entry.Recommendations = dto.Recommendations?.Trim();
            entry.Notes = dto.Notes?.Trim();

            _unitOfWork
                .Repository<MedicalReportEntry>()
                .Update(entry);

            foreach (var prescriptionDto in dto.Prescriptions)
            {
                var prescription = await _unitOfWork
                    .Repository<Prescription>()
                    .GetByIdAsync(prescriptionDto.PrescriptionId);

                if (prescription is null)
                    throw new NotFoundException("Prescription not found.");

                if (prescription.MedicalReportEntryId != entry.Id)
                    throw new ForbiddenException("Prescription does not belong to this report entry.");

                prescription.DrugName = prescriptionDto.DrugName.Trim();
                prescription.Dose = prescriptionDto.Dose?.Trim();
                prescription.Duration = prescriptionDto.Duration?.Trim();
                prescription.Instructions = prescriptionDto.Instructions?.Trim();

                _unitOfWork
                    .Repository<Prescription>()
                    .Update(prescription);
            }

            await _unitOfWork.SaveChangesAsync();

            var result = await BuildMedicalReportEntryDtoAsync(entry);

            return ApiResponse<MedicalReportEntryDto>.Ok(
                result,
                "Medical report entry updated successfully.");
        }

        private async Task<MedicalReportDto> BuildMedicalReportDtoAsync(
            MedicalReport report,
            Patient patient)
        {
            var patientUser = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(patient.UserId);

            var entries = await _unitOfWork
                .Repository<MedicalReportEntry>()
                .GetAllAsync(e => e.MedicalReportId == report.Id);

            var entryDtos = new List<MedicalReportEntryDto>();

            foreach (var entry in entries.OrderByDescending(e => e.CreatedAt))
            {
                var entryDto = await BuildMedicalReportEntryDtoAsync(entry);
                entryDtos.Add(entryDto);
            }

            return new MedicalReportDto
            {
                MedicalReportId = report.Id,
                PatientId = report.PatientId,
                PatientName = patientUser?.FullName ?? string.Empty,
                CreatedAt = report.CreatedAt,
                UpdatedAt = report.UpdatedAt,
                Entries = entryDtos
            };
        }

        private async Task<MedicalReportEntryDto> BuildMedicalReportEntryDtoAsync(
            MedicalReportEntry entry)
        {
            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(entry.DoctorId);

            User? doctorUser = null;

            if (doctor is not null)
            {
                doctorUser = await _unitOfWork
                    .Repository<User>()
                    .GetByIdAsync(doctor.UserId);
            }

            var prescriptions = await _unitOfWork
                .Repository<Prescription>()
                .GetAllAsync(p => p.MedicalReportEntryId == entry.Id);

            var prescriptionDtos = prescriptions
                .Select(p => new PrescriptionDto
                {
                    PrescriptionId = p.Id,
                    DrugName = p.DrugName,
                    Dose = p.Dose,
                    Duration = p.Duration,
                    Instructions = p.Instructions
                })
                .ToList();

            return new MedicalReportEntryDto
            {
                EntryId = entry.Id,
                AppointmentId = entry.AppointmentId,
                DoctorId = entry.DoctorId,
                DoctorName = doctorUser?.FullName ?? string.Empty,
                Diagnosis = entry.Diagnosis,
                Recommendations = entry.Recommendations,
                Notes = entry.Notes,
                CreatedAt = entry.CreatedAt,
                Prescriptions = prescriptionDtos
            };
        }
    }
}
