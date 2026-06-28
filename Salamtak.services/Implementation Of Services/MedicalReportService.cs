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

        private readonly IValidator<CreateMedicalReportEntryDto>
            _createValidator;

        private readonly IValidator<UpdateMedicalReportEntryDto>
            _updateValidator;

        public MedicalReportService(
            IUnitOfWork unitOfWork,
            IValidator<CreateMedicalReportEntryDto> createValidator,
            IValidator<UpdateMedicalReportEntryDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        // =========================================================
        // Patient: Get his own medical report
        // =========================================================

        public async Task<ApiResponse<MedicalReportDto>>
            GetMyReportAsync(Guid patientUserId)
        {
            var patient = await GetPatientByUserIdAsync(patientUserId);

            var report = await _unitOfWork
                .Repository<MedicalReport>()
                .FirstOrDefaultAsync(r =>
                    r.PatientId == patient.Id);

            if (report is null)
            {
                throw new NotFoundException(
                    "Medical report not found.");
            }

            var result = await BuildMedicalReportDtoAsync(
                report,
                patient);

            return ApiResponse<MedicalReportDto>.Ok(
                result,
                "Medical report retrieved successfully.");
        }

        // =========================================================
        // Doctor: Get patient report during appointment only
        // =========================================================

        public async Task<ApiResponse<MedicalReportDto>>
            GetPatientReportForDoctorAsync(
                Guid doctorUserId,
                Guid appointmentId)
        {
            var doctor = await GetDoctorByUserIdAsync(
                doctorUserId);

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
                    "You are not allowed to access this appointment.");
            }

            if (appointment.Status != AppointmentStatus.Confirmed)
            {
                throw new ForbiddenException(
                    "Only confirmed appointments allow access to medical reports.");
            }

            var slot = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .GetByIdAsync(appointment.AvailabilitySlotId);

            if (slot is null)
            {
                throw new NotFoundException(
                    "Appointment availability slot not found.");
            }

            var currentUtcTime = DateTime.UtcNow;

            var isDuringAppointment =
                currentUtcTime >= slot.StartTime &&
                currentUtcTime <= slot.EndTime;

            if (!isDuringAppointment)
            {
                throw new ForbiddenException(
                    "The medical report can only be accessed during the appointment time.");
            }

            var patient = await _unitOfWork
                .Repository<Patient>()
                .GetByIdAsync(appointment.PatientId);

            if (patient is null)
            {
                throw new NotFoundException(
                    "Patient profile not found.");
            }

            var report = await _unitOfWork
                .Repository<MedicalReport>()
                .FirstOrDefaultAsync(r =>
                    r.PatientId == patient.Id);

            if (report is null)
            {
                throw new NotFoundException(
                    "Medical report not found.");
            }

            var result = await BuildMedicalReportDtoAsync(
                report,
                patient);

            return ApiResponse<MedicalReportDto>.Ok(
                result,
                "Medical report retrieved successfully.");
        }

        // =========================================================
        // Doctor: Add report entry after completing appointment
        // =========================================================

        public async Task<ApiResponse<MedicalReportEntryDto>>
            AddEntryAsync(
                Guid doctorUserId,
                CreateMedicalReportEntryDto dto)
        {
            var validationResult =
                await _createValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors
                        .Select(e => e.ErrorMessage));
            }

            var doctor = await GetDoctorByUserIdAsync(
                doctorUserId);

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
                    "You are not allowed to add a report entry for this appointment.");
            }

            if (appointment.Status != AppointmentStatus.Completed)
            {
                throw new BadRequestException(
                    "Medical report entries can only be added after completing the appointment.");
            }

            var entryAlreadyExists = await _unitOfWork
                .Repository<MedicalReportEntry>()
                .AnyAsync(e =>
                    e.AppointmentId == appointment.Id);

            if (entryAlreadyExists)
            {
                throw new ConflictException(
                    "A medical report entry already exists for this appointment.");
            }

            var patientExists = await _unitOfWork
                .Repository<Patient>()
                .AnyAsync(p =>
                    p.Id == appointment.PatientId);

            if (!patientExists)
            {
                throw new NotFoundException(
                    "Patient profile not found.");
            }

            var report = await _unitOfWork
                .Repository<MedicalReport>()
                .FirstOrDefaultAsync(r =>
                    r.PatientId == appointment.PatientId);

            if (report is null)
            {
                report = new MedicalReport
                {
                    PatientId = appointment.PatientId
                };

                await _unitOfWork
                    .Repository<MedicalReport>()
                    .AddAsync(report);

                await _unitOfWork.SaveChangesAsync();
            }

            var entry = new MedicalReportEntry
            {
                MedicalReportId = report.Id,
                AppointmentId = appointment.Id,
                DoctorId = doctor.Id,
                Diagnosis = dto.Diagnosis?.Trim(),
                Recommendations = dto.Recommendations?.Trim(),
                Notes = dto.Notes?.Trim()
            };

            await _unitOfWork
                .Repository<MedicalReportEntry>()
                .AddAsync(entry);

            await _unitOfWork.SaveChangesAsync();

            if (dto.Prescriptions is not null)
            {
                foreach (var prescriptionDto in dto.Prescriptions)
                {
                    var prescription = new Prescription
                    {
                        MedicalReportEntryId = entry.Id,

                        DrugName =
                            prescriptionDto.DrugName.Trim(),

                        Dose =
                            prescriptionDto.Dose?.Trim(),

                        Duration =
                            prescriptionDto.Duration?.Trim(),

                        Instructions =
                            prescriptionDto.Instructions?.Trim()
                    };

                    await _unitOfWork
                        .Repository<Prescription>()
                        .AddAsync(prescription);
                }

                await _unitOfWork.SaveChangesAsync();
            }

            var result =
                await BuildMedicalReportEntryDtoAsync(entry);

            return ApiResponse<MedicalReportEntryDto>.Ok(
                result,
                "Medical report entry added successfully.");
        }

        // =========================================================
        // Doctor: Update his own report entry
        // =========================================================

        public async Task<ApiResponse<MedicalReportEntryDto>>
            UpdateEntryAsync(
                Guid doctorUserId,
                UpdateMedicalReportEntryDto dto)
        {
            var validationResult =
                await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors
                        .Select(e => e.ErrorMessage));
            }

            var doctor = await GetDoctorByUserIdAsync(
                doctorUserId);

            var entry = await _unitOfWork
                .Repository<MedicalReportEntry>()
                .GetByIdAsync(dto.EntryId);

            if (entry is null)
            {
                throw new NotFoundException(
                    "Medical report entry not found.");
            }

            if (entry.DoctorId != doctor.Id)
            {
                throw new ForbiddenException(
                    "You are not allowed to update this medical report entry.");
            }

            var appointment = await _unitOfWork
                .Repository<Appointment>()
                .GetByIdAsync(entry.AppointmentId);

            if (appointment is null)
            {
                throw new NotFoundException(
                    "Related appointment not found.");
            }

            if (appointment.DoctorId != doctor.Id)
            {
                throw new ForbiddenException(
                    "You are not allowed to update this appointment report.");
            }

            if (appointment.Status != AppointmentStatus.Completed)
            {
                throw new BadRequestException(
                    "Only completed appointment report entries can be updated.");
            }

            entry.Diagnosis = dto.Diagnosis?.Trim();

            entry.Recommendations =
                dto.Recommendations?.Trim();

            entry.Notes = dto.Notes?.Trim();

            _unitOfWork
                .Repository<MedicalReportEntry>()
                .Update(entry);

            if (dto.Prescriptions is not null)
            {
                foreach (var prescriptionDto in dto.Prescriptions)
                {
                    var prescription = await _unitOfWork
                        .Repository<Prescription>()
                        .GetByIdAsync(
                            prescriptionDto.PrescriptionId);

                    if (prescription is null)
                    {
                        throw new NotFoundException(
                            "Prescription not found.");
                    }

                    if (prescription.MedicalReportEntryId
                        != entry.Id)
                    {
                        throw new ForbiddenException(
                            "Prescription does not belong to this medical report entry.");
                    }

                    prescription.DrugName =
                        prescriptionDto.DrugName.Trim();

                    prescription.Dose =
                        prescriptionDto.Dose?.Trim();

                    prescription.Duration =
                        prescriptionDto.Duration?.Trim();

                    prescription.Instructions =
                        prescriptionDto.Instructions?.Trim();

                    _unitOfWork
                        .Repository<Prescription>()
                        .Update(prescription);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var result =
                await BuildMedicalReportEntryDtoAsync(entry);

            return ApiResponse<MedicalReportEntryDto>.Ok(
                result,
                "Medical report entry updated successfully.");
        }

        // =========================================================
        // Resolve Patient from JWT UserId
        // =========================================================

        private async Task<Patient> GetPatientByUserIdAsync(
            Guid patientUserId)
        {
            var patient = await _unitOfWork
                .Repository<Patient>()
                .FirstOrDefaultAsync(p =>
                    p.UserId == patientUserId);

            if (patient is null)
            {
                throw new NotFoundException(
                    "Patient profile not found.");
            }

            return patient;
        }

        // =========================================================
        // Resolve Doctor from JWT UserId
        // =========================================================

        private async Task<Doctor> GetDoctorByUserIdAsync(
            Guid doctorUserId)
        {
            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(d =>
                    d.UserId == doctorUserId);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor profile not found.");
            }

            return doctor;
        }

        // =========================================================
        // Build full medical report DTO
        // =========================================================

        private async Task<MedicalReportDto>
            BuildMedicalReportDtoAsync(
                MedicalReport report,
                Patient patient)
        {
            var patientUser = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(patient.UserId);

            var entries = await _unitOfWork
                .Repository<MedicalReportEntry>()
                .GetAllAsync(e =>
                    e.MedicalReportId == report.Id);

            var entryDtos =
                new List<MedicalReportEntryDto>();

            foreach (var entry in entries
                         .OrderByDescending(e => e.CreatedAt))
            {
                var entryDto =
                    await BuildMedicalReportEntryDtoAsync(entry);

                entryDtos.Add(entryDto);
            }

            return new MedicalReportDto
            {
                MedicalReportId = report.Id,
                PatientId = report.PatientId,

                PatientName =
                    patientUser?.FullName ?? string.Empty,

                CreatedAt = report.CreatedAt,
                UpdatedAt = report.UpdatedAt,
                Entries = entryDtos
            };
        }

        // =========================================================
        // Build report entry DTO with doctor and prescriptions
        // =========================================================

        private async Task<MedicalReportEntryDto>
            BuildMedicalReportEntryDtoAsync(
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
                .GetAllAsync(p =>
                    p.MedicalReportEntryId == entry.Id);

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

                DoctorName =
                    doctorUser?.FullName ?? string.Empty,

                Diagnosis = entry.Diagnosis,
                Recommendations = entry.Recommendations,
                Notes = entry.Notes,
                CreatedAt = entry.CreatedAt,
                Prescriptions = prescriptionDtos
            };
        }
    }
}
