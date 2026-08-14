using Microsoft.AspNetCore.Http;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.Files;
using Salamtak.Shared.DTOs.MedicalReports;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Implementation_Of_Services
{
    public class MedicalReportAttachmentService
      : IMedicalReportAttachmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IPrivateFileStorageService
            _privateFileStorageService;

        private const long MaxFileSizeInBytes =
            10 * 1024 * 1024;

        private static readonly HashSet<string>
            AllowedExtensions =
                new(StringComparer.OrdinalIgnoreCase)
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp",
                    ".pdf"
                };

        private static readonly HashSet<string>
            AllowedContentTypes =
                new(StringComparer.OrdinalIgnoreCase)
                {
                    "image/jpeg",
                    "image/png",
                    "image/webp",
                    "application/pdf"
                };

        public MedicalReportAttachmentService(
            IUnitOfWork unitOfWork,
            IPrivateFileStorageService
                privateFileStorageService)
        {
            _unitOfWork =
                unitOfWork;

            _privateFileStorageService =
                privateFileStorageService;
        }

       
        public async Task<
            ApiResponse<MedicalReportAttachmentDto>>
            UploadPatientAttachmentAsync(
                Guid patientUserId,
                UploadPatientMedicalAttachmentDto dto,
                CancellationToken cancellationToken = default)
        {
            ValidateFile(dto.File);
            ValidateDescription(dto.Description);

            var patient =
                await GetPatientByUserIdAsync(
                    patientUserId);

            var medicalReport = await _unitOfWork
                .Repository<MedicalReport>()
                .FirstOrDefaultAsync(r =>
                    r.PatientId == patient.Id &&
                    !r.IsDeleted);

            if (medicalReport is null)
            {
                throw new NotFoundException(
                    "Medical report not found.");
            }

            var folderName =
                $"medical-attachments/patients/{patient.Id}";

            var storedFile =
                await _privateFileStorageService
                    .UploadAsync(
                        dto.File,
                        folderName,
                        cancellationToken);

            var attachment =
                new MedicalReportAttachment
                {
                    MedicalReportId =
                        medicalReport.Id,

                    MedicalReportEntryId =
                        null,

                    UploadedByUserId =
                        patientUserId,

                    UploadedByType =
                        AttachmentUploaderType.Patient,

                    OriginalFileName =
                        storedFile.OriginalFileName,

                    StoredFileName =
                        storedFile.StoredFileName,

                    StoragePath =
                        storedFile.RelativePath,

                    ContentType =
                        storedFile.ContentType,

                    FileSize =
                        storedFile.FileSize,

                    Description =
                        NormalizeDescription(
                            dto.Description)
                };

            try
            {
                await _unitOfWork
                    .Repository<MedicalReportAttachment>()
                    .AddAsync(attachment);

                await _unitOfWork.SaveChangesAsync();
            }
            catch
            {
                await _privateFileStorageService
                    .DeleteAsync(
                        storedFile.RelativePath,
                        cancellationToken);

                throw;
            }

            return ApiResponse<
                MedicalReportAttachmentDto>.Ok(
                    MapToDto(attachment),
                    "Medical attachment uploaded successfully.");
        }


        public async Task<
            ApiResponse<MedicalReportAttachmentDto>>
            UploadDoctorAttachmentAsync(
                Guid doctorUserId,
                Guid medicalReportEntryId,
                UploadDoctorMedicalAttachmentDto dto,
                CancellationToken cancellationToken = default)
        {
            ValidateFile(dto.File);
            ValidateDescription(dto.Description);

            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            var entry = await _unitOfWork
                .Repository<MedicalReportEntry>()
                .GetByIdAsync(
                    medicalReportEntryId);

            if (entry is null ||
                entry.IsDeleted)
            {
                throw new NotFoundException(
                    "Medical report entry not found.");
            }

            if (entry.DoctorId != doctor.Id)
            {
                throw new ForbiddenException(
                    "You cannot upload a file to an entry created by another doctor.");
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
                    "You cannot upload a file for this appointment.");
            }

            if (appointment.Status !=
                AppointmentStatus.Completed)
            {
                throw new BadRequestException(
                    "Doctor attachments can only be uploaded after completing the appointment.");
            }

            var medicalReport = await _unitOfWork
                .Repository<MedicalReport>()
                .GetByIdAsync(entry.MedicalReportId);

            if (medicalReport is null ||
                medicalReport.IsDeleted)
            {
                throw new NotFoundException(
                    "Medical report not found.");
            }

            if (appointment.PatientId !=
                medicalReport.PatientId)
            {
                throw new BadRequestException(
                    "The appointment patient does not match the medical report patient.");
            }

            var folderName =
                $"medical-attachments/patients/" +
                $"{medicalReport.PatientId}/entries/{entry.Id}";

            var storedFile =
                await _privateFileStorageService
                    .UploadAsync(
                        dto.File,
                        folderName,
                        cancellationToken);

            var attachment =
                new MedicalReportAttachment
                {
                    MedicalReportId =
                        medicalReport.Id,

                    MedicalReportEntryId =
                        entry.Id,

                    UploadedByUserId =
                        doctorUserId,

                    UploadedByType =
                        AttachmentUploaderType.Doctor,

                    OriginalFileName =
                        storedFile.OriginalFileName,

                    StoredFileName =
                        storedFile.StoredFileName,

                    StoragePath =
                        storedFile.RelativePath,

                    ContentType =
                        storedFile.ContentType,

                    FileSize =
                        storedFile.FileSize,

                    Description =
                        NormalizeDescription(
                            dto.Description)
                };

            try
            {
                await _unitOfWork
                    .Repository<MedicalReportAttachment>()
                    .AddAsync(attachment);

                await _unitOfWork.SaveChangesAsync();
            }
            catch
            {
                await _privateFileStorageService
                    .DeleteAsync(
                        storedFile.RelativePath,
                        cancellationToken);

                throw;
            }

            return ApiResponse<
                MedicalReportAttachmentDto>.Ok(
                    MapToDto(attachment),
                    "Medical attachment uploaded successfully.");
        }

       
        public async Task<
            ApiResponse<IReadOnlyList<
                MedicalReportAttachmentDto>>>
            GetMyAttachmentsAsync(
                Guid patientUserId,
                CancellationToken cancellationToken = default)
        {
            var patient =
                await GetPatientByUserIdAsync(
                    patientUserId);

            var medicalReport = await _unitOfWork
                .Repository<MedicalReport>()
                .FirstOrDefaultAsync(r =>
                    r.PatientId == patient.Id &&
                    !r.IsDeleted);

            if (medicalReport is null)
            {
                throw new NotFoundException(
                    "Medical report not found.");
            }

            var attachments = await _unitOfWork
                .Repository<MedicalReportAttachment>()
                .GetAllAsync(a =>
                    a.MedicalReportId ==
                    medicalReport.Id &&
                    !a.IsDeleted);

            IReadOnlyList<
                MedicalReportAttachmentDto> result =
                    attachments
                        .OrderByDescending(a =>
                            a.CreatedAt)
                        .Select(MapToDto)
                        .ToList();

            return ApiResponse<
                IReadOnlyList<
                    MedicalReportAttachmentDto>>.Ok(
                        result,
                        "Medical attachments retrieved successfully.");
        }


        public async Task<
            ApiResponse<IReadOnlyList<
                MedicalReportAttachmentDto>>>
            GetPatientAttachmentsForDoctorAsync(
                Guid doctorUserId,
                Guid appointmentId,
                CancellationToken cancellationToken = default)
        {
            var access =
                await ValidateDoctorAppointmentSlotAccessAsync(
                    doctorUserId,
                    appointmentId);

            var medicalReport = await _unitOfWork
                .Repository<MedicalReport>()
                .FirstOrDefaultAsync(r =>
                    r.PatientId ==
                    access.Appointment.PatientId &&
                    !r.IsDeleted);

            if (medicalReport is null)
            {
                throw new NotFoundException(
                    "Medical report not found.");
            }

            var attachments = await _unitOfWork
                .Repository<MedicalReportAttachment>()
                .GetAllAsync(a =>
                    a.MedicalReportId ==
                    medicalReport.Id &&
                    !a.IsDeleted);

            IReadOnlyList<
                MedicalReportAttachmentDto> result =
                    attachments
                        .OrderByDescending(a =>
                            a.CreatedAt)
                        .Select(MapToDto)
                        .ToList();

            return ApiResponse<
                IReadOnlyList<
                    MedicalReportAttachmentDto>>.Ok(
                        result,
                        "Medical attachments retrieved successfully.");
        }


        public async Task<PrivateFileDownloadResult>
            DownloadPatientAttachmentAsync(
                Guid patientUserId,
                Guid attachmentId,
                CancellationToken cancellationToken = default)
        {
            var patient =
                await GetPatientByUserIdAsync(
                    patientUserId);

            var attachment =
                await GetAttachmentAsync(
                    attachmentId);

            var medicalReport = await _unitOfWork
                .Repository<MedicalReport>()
                .GetByIdAsync(
                    attachment.MedicalReportId);

            if (medicalReport is null ||
                medicalReport.IsDeleted)
            {
                throw new NotFoundException(
                    "Medical report not found.");
            }

            if (medicalReport.PatientId != patient.Id)
            {
                throw new ForbiddenException(
                    "You are not allowed to download this attachment.");
            }

            return await BuildDownloadResultAsync(
                attachment,
                cancellationToken);
        }

        public async Task<PrivateFileDownloadResult>
            DownloadDoctorAttachmentAsync(
                Guid doctorUserId,
                Guid appointmentId,
                Guid attachmentId,
                CancellationToken cancellationToken = default)
        {
            var access =
                await ValidateDoctorAppointmentSlotAccessAsync(
                    doctorUserId,
                    appointmentId);

            var attachment =
                await GetAttachmentAsync(
                    attachmentId);

            var medicalReport = await _unitOfWork
                .Repository<MedicalReport>()
                .GetByIdAsync(
                    attachment.MedicalReportId);

            if (medicalReport is null ||
                medicalReport.IsDeleted)
            {
                throw new NotFoundException(
                    "Medical report not found.");
            }

            if (medicalReport.PatientId !=
                access.Appointment.PatientId)
            {
                throw new ForbiddenException(
                    "This attachment does not belong to the appointment patient.");
            }

            return await BuildDownloadResultAsync(
                attachment,
                cancellationToken);
        }

        public async Task<ApiResponse<bool>>
            DeletePatientAttachmentAsync(
                Guid patientUserId,
                Guid attachmentId,
                CancellationToken cancellationToken = default)
        {
            var patient =
                await GetPatientByUserIdAsync(
                    patientUserId);

            var attachment =
                await GetAttachmentAsync(
                    attachmentId);

            var medicalReport = await _unitOfWork
                .Repository<MedicalReport>()
                .GetByIdAsync(
                    attachment.MedicalReportId);

            if (medicalReport is null ||
                medicalReport.IsDeleted)
            {
                throw new NotFoundException(
                    "Medical report not found.");
            }

            if (medicalReport.PatientId != patient.Id)
            {
                throw new ForbiddenException(
                    "You are not allowed to delete this attachment.");
            }

            if (attachment.UploadedByUserId !=
                    patientUserId ||
                attachment.UploadedByType !=
                    AttachmentUploaderType.Patient)
            {
                throw new ForbiddenException(
                    "You can only delete attachments uploaded by you.");
            }

            return await DeleteAttachmentInternalAsync(
                attachment,
                cancellationToken);
        }

        public async Task<ApiResponse<bool>>
            DeleteDoctorAttachmentAsync(
                Guid doctorUserId,
                Guid attachmentId,
                CancellationToken cancellationToken = default)
        {
            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            var attachment =
                await GetAttachmentAsync(
                    attachmentId);

            if (attachment.UploadedByUserId !=
                    doctorUserId ||
                attachment.UploadedByType !=
                    AttachmentUploaderType.Doctor)
            {
                throw new ForbiddenException(
                    "You can only delete attachments uploaded by you.");
            }

            if (!attachment.MedicalReportEntryId.HasValue)
            {
                throw new BadRequestException(
                    "Doctor attachment must be linked to a medical report entry.");
            }

            var entry = await _unitOfWork
                .Repository<MedicalReportEntry>()
                .GetByIdAsync(
                    attachment
                        .MedicalReportEntryId
                        .Value);

            if (entry is null ||
                entry.IsDeleted)
            {
                throw new NotFoundException(
                    "Medical report entry not found.");
            }

            if (entry.DoctorId != doctor.Id)
            {
                throw new ForbiddenException(
                    "You cannot delete an attachment from another doctor's entry.");
            }

            return await DeleteAttachmentInternalAsync(
                attachment,
                cancellationToken);
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
                    "Only confirmed appointments allow access to medical attachments.");
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
                    "Medical attachments can only be accessed during the appointment time.");
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

        private async Task<ApiResponse<bool>>
            DeleteAttachmentInternalAsync(
                MedicalReportAttachment attachment,
                CancellationToken cancellationToken)
        {
            var storedPath =
                attachment.StoragePath;

            _unitOfWork
                .Repository<MedicalReportAttachment>()
                .SoftDelete(attachment);

            await _unitOfWork.SaveChangesAsync();

            await _privateFileStorageService
                .DeleteAsync(
                    storedPath,
                    cancellationToken);

            return ApiResponse<bool>.Ok(
                true,
                "Medical attachment deleted successfully.");
        }

        private async Task<PrivateFileDownloadResult>
            BuildDownloadResultAsync(
                MedicalReportAttachment attachment,
                CancellationToken cancellationToken)
        {
            if (!_privateFileStorageService.Exists(
                    attachment.StoragePath))
            {
                throw new NotFoundException(
                    "Attachment file was not found.");
            }

            var stream =
                await _privateFileStorageService
                    .OpenReadAsync(
                        attachment.StoragePath,
                        cancellationToken);

            return new PrivateFileDownloadResult
            {
                Stream =
                    stream,

                ContentType =
                    string.IsNullOrWhiteSpace(
                        attachment.ContentType)
                        ? "application/octet-stream"
                        : attachment.ContentType,

                FileName =
                    attachment.OriginalFileName
            };
        }

        private async Task<MedicalReportAttachment>
            GetAttachmentAsync(
                Guid attachmentId)
        {
            var attachment = await _unitOfWork
                .Repository<MedicalReportAttachment>()
                .GetByIdAsync(attachmentId);

            if (attachment is null ||
                attachment.IsDeleted)
            {
                throw new NotFoundException(
                    "Medical attachment not found.");
            }

            return attachment;
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

        private static void ValidateFile(
            IFormFile file)
        {
            if (file is null ||
                file.Length == 0)
            {
                throw new BadRequestException(
                    "Medical attachment file is required.");
            }

            if (file.Length >
                MaxFileSizeInBytes)
            {
                throw new BadRequestException(
                    "Medical attachment size must not exceed 10 MB.");
            }

            var extension =
                Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(
                    extension) ||
                !AllowedExtensions.Contains(
                    extension))
            {
                throw new BadRequestException(
                    "Only JPG, JPEG, PNG, WEBP and PDF files are allowed.");
            }

            var contentType =
                file.ContentType?.Trim();

            if (string.IsNullOrWhiteSpace(
                    contentType) ||
                !AllowedContentTypes.Contains(
                    contentType))
            {
                throw new BadRequestException(
                    "Invalid medical attachment content type.");
            }
        }

        private static void ValidateDescription(
            string? description)
        {
            if (description?.Trim().Length > 500)
            {
                throw new BadRequestException(
                    "Description must not exceed 500 characters.");
            }
        }

        private static string? NormalizeDescription(
            string? description)
        {
            return string.IsNullOrWhiteSpace(
                description)
                ? null
                : description.Trim();
        }

        private static MedicalReportAttachmentDto
            MapToDto(
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

        private sealed class DoctorAppointmentAccess
        {
            public Doctor Doctor { get; set; } = null!;

            public Appointment Appointment { get; set; } = null!;

            public AvailabilitySlot Slot { get; set; } = null!;
        }
    }
}
