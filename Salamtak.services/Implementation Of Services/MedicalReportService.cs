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
            _unitOfWork =
                unitOfWork;

            _createValidator =
                createValidator;

            _updateValidator =
                updateValidator;
        }

       
        public async Task<ApiResponse<MedicalReportDto>>
            GetMyReportAsync(
                Guid patientUserId)
        {
            var patient =
                await GetPatientByUserIdAsync(
                    patientUserId);

            var report = await _unitOfWork
                .Repository<MedicalReport>()
                .FirstOrDefaultAsync(r =>
                    r.PatientId == patient.Id &&
                    !r.IsDeleted);

            if (report is null)
            {
                throw new NotFoundException(
                    "Medical report not found.");
            }

            var result =
                await BuildMedicalReportDtoAsync(
                    report,
                    patient);

            return ApiResponse<MedicalReportDto>.Ok(
                result,
                "Medical report retrieved successfully.");
        }

        public async Task<ApiResponse<MedicalReportDto>>
            GetPatientReportForDoctorAsync(
                Guid doctorUserId,
                Guid appointmentId)
        {
            var access =
                await ValidateDoctorAppointmentSlotAccessAsync(
                    doctorUserId,
                    appointmentId);

            var patient = await _unitOfWork
                .Repository<Patient>()
                .GetByIdAsync(
                    access.Appointment.PatientId);

            if (patient is null ||
                patient.IsDeleted)
            {
                throw new NotFoundException(
                    "Patient profile not found.");
            }

            var report = await _unitOfWork
                .Repository<MedicalReport>()
                .FirstOrDefaultAsync(r =>
                    r.PatientId == patient.Id &&
                    !r.IsDeleted);

            if (report is null)
            {
                throw new NotFoundException(
                    "Medical report not found.");
            }

            var result =
                await BuildMedicalReportDtoAsync(
                    report,
                    patient);

            return ApiResponse<MedicalReportDto>.Ok(
                result,
                "Medical report retrieved successfully.");
        }

       
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

            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            var appointment = await _unitOfWork
                .Repository<Appointment>()
                .GetByIdAsync(
                    dto.AppointmentId);

            if (appointment is null ||
                appointment.IsDeleted)
            {
                throw new NotFoundException(
                    "Appointment not found.");
            }

            if (appointment.DoctorId != doctor.Id)
            {
                throw new ForbiddenException(
                    "You are not allowed to add a medical report entry for this appointment.");
            }

            if (appointment.Status !=
                AppointmentStatus.Completed)
            {
                throw new BadRequestException(
                    "Medical report entries can only be added after completing the appointment.");
            }

            var entryAlreadyExists = await _unitOfWork
                .Repository<MedicalReportEntry>()
                .AnyAsync(e =>
                    e.AppointmentId == appointment.Id &&
                    !e.IsDeleted);

            if (entryAlreadyExists)
            {
                throw new ConflictException(
                    "A medical report entry already exists for this appointment.");
            }

            var patient = await _unitOfWork
                .Repository<Patient>()
                .GetByIdAsync(
                    appointment.PatientId);

            if (patient is null ||
                patient.IsDeleted)
            {
                throw new NotFoundException(
                    "Patient profile not found.");
            }

            var report = await _unitOfWork
                .Repository<MedicalReport>()
                .FirstOrDefaultAsync(r =>
                    r.PatientId == patient.Id &&
                    !r.IsDeleted);

            if (report is null)
            {
                report = new MedicalReport
                {
                    PatientId = patient.Id
                };

                await _unitOfWork
                    .Repository<MedicalReport>()
                    .AddAsync(report);

                await _unitOfWork.SaveChangesAsync();
            }

            var entry = new MedicalReportEntry
            {
                MedicalReportId =
                    report.Id,

                AppointmentId =
                    appointment.Id,

                DoctorId =
                    doctor.Id,

                Diagnosis =
                    NormalizeText(dto.Diagnosis),

                Recommendations =
                    NormalizeText(dto.Recommendations),

                Notes =
                    NormalizeText(dto.Notes)
            };

            await _unitOfWork
                .Repository<MedicalReportEntry>()
                .AddAsync(entry);

            await _unitOfWork.SaveChangesAsync();

            if (dto.Prescriptions is not null)
            {
                foreach (var prescriptionDto
                         in dto.Prescriptions)
                {
                    var prescription =
                        new Prescription
                        {
                            MedicalReportEntryId =
                                entry.Id,

                            DrugName =
                                prescriptionDto
                                    .DrugName
                                    .Trim(),

                            Dose =
                                NormalizeText(
                                    prescriptionDto.Dose),

                            Duration =
                                NormalizeText(
                                    prescriptionDto.Duration),

                            Instructions =
                                NormalizeText(
                                    prescriptionDto.Instructions)
                        };

                    await _unitOfWork
                        .Repository<Prescription>()
                        .AddAsync(prescription);
                }

                await _unitOfWork.SaveChangesAsync();
            }

            var result =
                await BuildMedicalReportEntryDtoAsync(
                    entry);

            return ApiResponse<MedicalReportEntryDto>.Ok(
                result,
                "Medical report entry added successfully.");
        }

       
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

            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            var entry = await _unitOfWork
                .Repository<MedicalReportEntry>()
                .GetByIdAsync(dto.EntryId);

            if (entry is null ||
                entry.IsDeleted)
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

            if (appointment is null ||
                appointment.IsDeleted)
            {
                throw new NotFoundException(
                    "Related appointment not found.");
            }

            if (appointment.DoctorId != doctor.Id)
            {
                throw new ForbiddenException(
                    "You are not allowed to update this appointment report.");
            }

            if (appointment.Status !=
                AppointmentStatus.Completed)
            {
                throw new BadRequestException(
                    "Only completed appointment report entries can be updated.");
            }

            entry.Diagnosis =
                NormalizeText(dto.Diagnosis);

            entry.Recommendations =
                NormalizeText(dto.Recommendations);

            entry.Notes =
                NormalizeText(dto.Notes);

            _unitOfWork
                .Repository<MedicalReportEntry>()
                .Update(entry);

            if (dto.Prescriptions is not null)
            {
                foreach (var prescriptionDto
                         in dto.Prescriptions)
                {
                    var prescription =
                        await _unitOfWork
                            .Repository<Prescription>()
                            .GetByIdAsync(
                                prescriptionDto
                                    .PrescriptionId);

                    if (prescription is null ||
                        prescription.IsDeleted)
                    {
                        throw new NotFoundException(
                            "Prescription not found.");
                    }

                    if (prescription
                            .MedicalReportEntryId !=
                        entry.Id)
                    {
                        throw new ForbiddenException(
                            "Prescription does not belong to this medical report entry.");
                    }

                    prescription.DrugName =
                        prescriptionDto
                            .DrugName
                            .Trim();

                    prescription.Dose =
                        NormalizeText(
                            prescriptionDto.Dose);

                    prescription.Duration =
                        NormalizeText(
                            prescriptionDto.Duration);

                    prescription.Instructions =
                        NormalizeText(
                            prescriptionDto.Instructions);

                    _unitOfWork
                        .Repository<Prescription>()
                        .Update(prescription);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var result =
                await BuildMedicalReportEntryDtoAsync(
                    entry);

            return ApiResponse<MedicalReportEntryDto>.Ok(
                result,
                "Medical report entry updated successfully.");
        }

        
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
                    e.MedicalReportId == report.Id &&
                    !e.IsDeleted);

            var attachments = await _unitOfWork
                .Repository<MedicalReportAttachment>()
                .GetAllAsync(a =>
                    a.MedicalReportId == report.Id &&
                    !a.IsDeleted);

            var attachmentList =
                attachments.ToList();

            var entryDtos =
                new List<MedicalReportEntryDto>();

            foreach (var entry in entries
                         .OrderByDescending(e =>
                             e.CreatedAt))
            {
                var entryDto =
                    await BuildMedicalReportEntryDtoAsync(
                        entry,
                        attachmentList);

                entryDtos.Add(entryDto);
            }

            return new MedicalReportDto
            {
                MedicalReportId =
                    report.Id,

                PatientId =
                    report.PatientId,

                PatientName =
                    patientUser?.FullName ??
                    string.Empty,

                CreatedAt =
                    report.CreatedAt,

                UpdatedAt =
                    report.UpdatedAt,

                Attachments =
                    attachmentList
                        .Where(a =>
                            !a.MedicalReportEntryId
                                .HasValue)
                        .OrderByDescending(a =>
                            a.CreatedAt)
                        .Select(MapAttachmentToDto)
                        .ToList(),

                Entries =
                    entryDtos
            };
        }

       
        private async Task<MedicalReportEntryDto>
            BuildMedicalReportEntryDtoAsync(
                MedicalReportEntry entry,
                IReadOnlyCollection<MedicalReportAttachment>?
                    reportAttachments = null)
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
                    p.MedicalReportEntryId == entry.Id &&
                    !p.IsDeleted);

            var prescriptionDtos =
                prescriptions
                    .Select(p =>
                        new PrescriptionDto
                        {
                            PrescriptionId =
                                p.Id,

                            DrugName =
                                p.DrugName,

                            Dose =
                                p.Dose,

                            Duration =
                                p.Duration,

                            Instructions =
                                p.Instructions
                        })
                    .ToList();

            List<MedicalReportAttachmentDto>
                attachmentDtos;

            if (reportAttachments is null)
            {
                var attachments = await _unitOfWork
                    .Repository<MedicalReportAttachment>()
                    .GetAllAsync(a =>
                        a.MedicalReportEntryId ==
                        entry.Id &&
                        !a.IsDeleted);

                attachmentDtos =
                    attachments
                        .OrderByDescending(a =>
                            a.CreatedAt)
                        .Select(MapAttachmentToDto)
                        .ToList();
            }
            else
            {
                attachmentDtos =
                    reportAttachments
                        .Where(a =>
                            a.MedicalReportEntryId ==
                            entry.Id &&
                            !a.IsDeleted)
                        .OrderByDescending(a =>
                            a.CreatedAt)
                        .Select(MapAttachmentToDto)
                        .ToList();
            }

            return new MedicalReportEntryDto
            {
                EntryId =
                    entry.Id,

                AppointmentId =
                    entry.AppointmentId,

                DoctorId =
                    entry.DoctorId,

                DoctorName =
                    doctorUser?.FullName ??
                    string.Empty,

                Diagnosis =
                    entry.Diagnosis,

                Recommendations =
                    entry.Recommendations,

                Notes =
                    entry.Notes,

                CreatedAt =
                    entry.CreatedAt,

                Prescriptions =
                    prescriptionDtos,

                Attachments =
                    attachmentDtos
            };
        }

        
        private async Task<DoctorAppointmentAccess>
            ValidateDoctorAppointmentSlotAccessAsync(
                Guid doctorUserId,
                Guid appointmentId)
        {
            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            var appointment = await _unitOfWork
                .Repository<Appointment>()
                .GetByIdAsync(appointmentId);

            if (appointment is null ||
                appointment.IsDeleted)
            {
                throw new NotFoundException(
                    "Appointment not found.");
            }

            if (appointment.DoctorId != doctor.Id)
            {
                throw new ForbiddenException(
                    "You are not allowed to access this appointment.");
            }

            if (appointment.Status !=
                AppointmentStatus.Confirmed)
            {
                throw new ForbiddenException(
                    "Only confirmed appointments allow access to medical reports.");
            }

            var slot = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .GetByIdAsync(
                    appointment.AvailabilitySlotId);

            if (slot is null ||
                slot.IsDeleted)
            {
                throw new NotFoundException(
                    "Appointment availability slot not found.");
            }

            var currentUtcTime =
                DateTime.UtcNow;

            if (currentUtcTime < slot.StartTime ||
                currentUtcTime > slot.EndTime)
            {
                throw new ForbiddenException(
                    "The medical report can only be accessed during the appointment time.");
            }

            return new DoctorAppointmentAccess
            {
                Doctor =
                    doctor,

                Appointment =
                    appointment,

                Slot =
                    slot
            };
        }

        private async Task<Patient>
            GetPatientByUserIdAsync(
                Guid patientUserId)
        {
            var patient = await _unitOfWork
                .Repository<Patient>()
                .FirstOrDefaultAsync(p =>
                    p.UserId == patientUserId &&
                    !p.IsDeleted);

            if (patient is null)
            {
                throw new NotFoundException(
                    "Patient profile not found.");
            }

            return patient;
        }

        private async Task<Doctor>
            GetDoctorByUserIdAsync(
                Guid doctorUserId)
        {
            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(d =>
                    d.UserId == doctorUserId &&
                    !d.IsDeleted);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor profile not found.");
            }

            return doctor;
        }

        private static MedicalReportAttachmentDto
            MapAttachmentToDto(
                MedicalReportAttachment attachment)
        {
            return new MedicalReportAttachmentDto
            {
                Id =
                    attachment.Id,

                MedicalReportId =
                    attachment.MedicalReportId,

                MedicalReportEntryId =
                    attachment.MedicalReportEntryId,

                UploadedByUserId =
                    attachment.UploadedByUserId,

                UploadedByType =
                    attachment.UploadedByType
                        .ToString(),

                OriginalFileName =
                    attachment.OriginalFileName,

                ContentType =
                    attachment.ContentType,

                FileSize =
                    attachment.FileSize,

                Description =
                    attachment.Description,

                CreatedAt =
                    attachment.CreatedAt
            };
        }

        private static string? NormalizeText(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private sealed class DoctorAppointmentAccess
        {
            public Doctor Doctor { get; set; } = null!;

            public Appointment Appointment { get; set; } = null!;

            public AvailabilitySlot Slot { get; set; } = null!;
        }
    }
}
