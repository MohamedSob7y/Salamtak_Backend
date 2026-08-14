using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.DoctorDocuments;
using Salamtak.Shared.DTOs.Files;
using Salamtak.Shared.Responses;
using System.Linq;

namespace Salamtak.services.Implementation_Of_Services
{
    public class DoctorDocumentService
        : IDoctorDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IPrivateFileStorageService
            _privateFileStorageService;

        private readonly IValidator<UploadDoctorDocumentDto>
            _uploadValidator;

        private readonly IValidator<RejectDoctorDocumentDto>
            _rejectValidator;

        private const long MaxFileSizeInBytes =
            10 * 1024 * 1024;

        private static readonly HashSet<string>
            AllowedExtensions =
                new(StringComparer.OrdinalIgnoreCase)
                {
                    ".pdf",
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

        private static readonly HashSet<string>
            AllowedContentTypes =
                new(StringComparer.OrdinalIgnoreCase)
                {
                    "application/pdf",
                    "image/jpeg",
                    "image/png",
                    "image/webp"
                };

        public DoctorDocumentService(
            IUnitOfWork unitOfWork,
            IPrivateFileStorageService privateFileStorageService,
            IValidator<UploadDoctorDocumentDto> uploadValidator,
            IValidator<RejectDoctorDocumentDto> rejectValidator)
        {
            _unitOfWork =
                unitOfWork;

            _privateFileStorageService =
                privateFileStorageService;

            _uploadValidator =
                uploadValidator;

            _rejectValidator =
                rejectValidator;
        }

      
        public async Task<ApiResponse<DoctorDocumentDto>>
            UploadMyDocumentAsync(
                Guid doctorUserId,
                UploadDoctorDocumentDto dto,
                CancellationToken cancellationToken = default)
        {
            var validationResult =
                await _uploadValidator.ValidateAsync(
                    dto,
                    cancellationToken);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors
                        .Select(e => e.ErrorMessage));
            }

            ValidateFile(dto.File);

            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            if (!Enum.TryParse<DoctorDocumentType>(
                    dto.DocumentType,
                    true,
                    out var documentType))
            {
                throw new BadRequestException(
                    "Invalid document type.");
            }

            var sameDocumentExists = await _unitOfWork
                .Repository<DoctorDocument>()
                .AnyAsync(d =>
                    d.DoctorId == doctor.Id &&
                    d.DocumentType == documentType &&
                    !d.IsDeleted);

            if (sameDocumentExists)
            {
                throw new ConflictException(
                    $"A {documentType} document already exists.");
            }

            var folderName =
                $"doctor-documents/{doctor.Id}";

            var storedFile =
                await _privateFileStorageService.UploadAsync(
                    dto.File,
                    folderName,
                    cancellationToken);

            var document = new DoctorDocument
            {
                DoctorId =
                    doctor.Id,

                DocumentType =
                    documentType,

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
                        dto.Description),

                IsVerified =
                    false,

                RejectionReason =
                    null,

                VerifiedAt =
                    null,

                VerifiedByAdminId =
                    null
            };

            doctor.IsVerified = false;

            doctor.VerificationStatus =
                DoctorVerificationStatus.Pending;

            try
            {
                await _unitOfWork
                    .Repository<DoctorDocument>()
                    .AddAsync(document);

                _unitOfWork
                    .Repository<Doctor>()
                    .Update(doctor);

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

            var result =
                await BuildDoctorDocumentDtoAsync(
                    document);

            return ApiResponse<DoctorDocumentDto>.Ok(
                result,
                "Document uploaded successfully.");
        }

        public async Task<
            ApiResponse<IReadOnlyList<DoctorDocumentDto>>>
            GetMyDocumentsAsync(
                Guid doctorUserId,
                CancellationToken cancellationToken = default)
        {
            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            var documents = await _unitOfWork
                .Repository<DoctorDocument>()
                .GetAllAsync(d =>
                    d.DoctorId == doctor.Id &&
                    !d.IsDeleted);

            var result = new List<DoctorDocumentDto>();

            foreach (var document in documents
                         .OrderByDescending(d =>
                             d.CreatedAt))
            {
                result.Add(
                    await BuildDoctorDocumentDtoAsync(
                        document));
            }

            return ApiResponse<
                IReadOnlyList<DoctorDocumentDto>>.Ok(
                    result,
                    "Doctor documents retrieved successfully.");
        }

        public async Task<PrivateFileDownloadResult>
            DownloadMyDocumentAsync(
                Guid doctorUserId,
                Guid documentId,
                CancellationToken cancellationToken = default)
        {
            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            var document =
                await GetDocumentAsync(
                    documentId);

            if (document.DoctorId != doctor.Id)
            {
                throw new ForbiddenException(
                    "You are not allowed to download this document.");
            }

            return await BuildDownloadResultAsync(
                document,
                cancellationToken);
        }

        public async Task<ApiResponse<bool>>
            DeleteMyDocumentAsync(
                Guid doctorUserId,
                Guid documentId,
                CancellationToken cancellationToken = default)
        {
            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            var document =
                await GetDocumentAsync(
                    documentId);

            if (document.DoctorId != doctor.Id)
            {
                throw new ForbiddenException(
                    "You are not allowed to delete this document.");
            }

            if (document.IsVerified)
            {
                throw new ConflictException(
                    "A verified document cannot be deleted.");
            }

            var storagePath =
                document.StoragePath;

            _unitOfWork
                .Repository<DoctorDocument>()
                .SoftDelete(document);

            doctor.IsVerified = false;

            doctor.VerificationStatus =
                DoctorVerificationStatus.Pending;

            _unitOfWork
                .Repository<Doctor>()
                .Update(doctor);

            await _unitOfWork.SaveChangesAsync();

            await _privateFileStorageService
                .DeleteAsync(
                    storagePath,
                    cancellationToken);

            return ApiResponse<bool>.Ok(
                true,
                "Document deleted successfully.");
        }

      
        public async Task<
            ApiResponse<IReadOnlyList<DoctorDocumentDto>>>
            GetDoctorDocumentsForAdminAsync(
                Guid adminUserId,
                Guid doctorId,
                CancellationToken cancellationToken = default)
        {
            await GetAdminByUserIdAsync(
                adminUserId);

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(doctorId);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor not found.");
            }

            var documents = await _unitOfWork
                .Repository<DoctorDocument>()
                .GetAllAsync(d =>
                    d.DoctorId == doctorId &&
                    !d.IsDeleted);

            var result = new List<DoctorDocumentDto>();

            foreach (var document in documents
                         .OrderByDescending(d =>
                             d.CreatedAt))
            {
                result.Add(
                    await BuildDoctorDocumentDtoAsync(
                        document));
            }

            return ApiResponse<
                IReadOnlyList<DoctorDocumentDto>>.Ok(
                    result,
                    "Doctor documents retrieved successfully.");
        }

        public async Task<PrivateFileDownloadResult>
            DownloadDocumentForAdminAsync(
                Guid adminUserId,
                Guid documentId,
                CancellationToken cancellationToken = default)
        {
            await GetAdminByUserIdAsync(
                adminUserId);

            var document =
                await GetDocumentAsync(
                    documentId);

            return await BuildDownloadResultAsync(
                document,
                cancellationToken);
        }

        public async Task<ApiResponse<DoctorDocumentDto>>
            ApproveDocumentAsync(
                Guid adminUserId,
                Guid documentId,
                CancellationToken cancellationToken = default)
        {
            var admin =
                await GetAdminByUserIdAsync(
                    adminUserId);

            var document =
                await GetDocumentAsync(
                    documentId);

            if (document.IsVerified)
            {
                throw new ConflictException(
                    "Document is already approved.");
            }

            document.IsVerified =
                true;

            document.VerifiedByAdminId =
                admin.Id;

            document.VerifiedAt =
                DateTime.UtcNow;

            document.RejectionReason =
                null;

            _unitOfWork
                .Repository<DoctorDocument>()
                .Update(document);

            await _unitOfWork.SaveChangesAsync();

            var result =
                await BuildDoctorDocumentDtoAsync(
                    document);

            return ApiResponse<DoctorDocumentDto>.Ok(
                result,
                "Document approved successfully.");
        }

        public async Task<ApiResponse<DoctorDocumentDto>>
            RejectDocumentAsync(
                Guid adminUserId,
                Guid documentId,
                RejectDoctorDocumentDto dto,
                CancellationToken cancellationToken = default)
        {
            var validationResult =
                await _rejectValidator.ValidateAsync(
                    dto,
                    cancellationToken);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors
                        .Select(e => e.ErrorMessage));
            }

            var admin =
                await GetAdminByUserIdAsync(
                    adminUserId);

            var document =
                await GetDocumentAsync(
                    documentId);

            document.IsVerified =
                false;

            document.VerifiedByAdminId =
                admin.Id;

            document.VerifiedAt =
                DateTime.UtcNow;

            document.RejectionReason =
                dto.RejectionReason.Trim();

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(document.DoctorId);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor not found.");
            }

            doctor.IsVerified =
                false;

            doctor.VerificationStatus =
                DoctorVerificationStatus.Rejected;

            _unitOfWork
                .Repository<DoctorDocument>()
                .Update(document);

            _unitOfWork
                .Repository<Doctor>()
                .Update(doctor);

            await _unitOfWork.SaveChangesAsync();

            var result =
                await BuildDoctorDocumentDtoAsync(
                    document);

            return ApiResponse<DoctorDocumentDto>.Ok(
                result,
                "Document rejected successfully.");
        }

       
        private async Task<Doctor>
            GetDoctorByUserIdAsync(
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

        private async Task<Admin>
            GetAdminByUserIdAsync(
                Guid adminUserId)
        {
            var admin = await _unitOfWork
                .Repository<Admin>()
                .FirstOrDefaultAsync(a =>
                    a.UserId == adminUserId);

            if (admin is null)
            {
                throw new NotFoundException(
                    "Admin profile not found.");
            }

            return admin;
        }

        private async Task<DoctorDocument>
            GetDocumentAsync(
                Guid documentId)
        {
            var document = await _unitOfWork
                .Repository<DoctorDocument>()
                .GetByIdAsync(documentId);

            if (document is null ||
                document.IsDeleted)
            {
                throw new NotFoundException(
                    "Doctor document not found.");
            }

            return document;
        }

        private async Task<PrivateFileDownloadResult>
            BuildDownloadResultAsync(
                DoctorDocument document,
                CancellationToken cancellationToken)
        {
            if (!_privateFileStorageService.Exists(
                    document.StoragePath))
            {
                throw new NotFoundException(
                    "Document file was not found.");
            }

            var stream =
                await _privateFileStorageService
                    .OpenReadAsync(
                        document.StoragePath,
                        cancellationToken);

            return new PrivateFileDownloadResult
            {
                Stream =
                    stream,

                ContentType =
                    string.IsNullOrWhiteSpace(
                        document.ContentType)
                        ? "application/octet-stream"
                        : document.ContentType,

                FileName =
                    document.OriginalFileName
            };
        }

        private async Task<DoctorDocumentDto>
            BuildDoctorDocumentDtoAsync(
                DoctorDocument document)
        {
            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(document.DoctorId);

            User? doctorUser = null;

            if (doctor is not null)
            {
                doctorUser = await _unitOfWork
                    .Repository<User>()
                    .GetByIdAsync(doctor.UserId);
            }

            return new DoctorDocumentDto
            {
                DocumentId =
                    document.Id,

                DoctorId =
                    document.DoctorId,

                DoctorName =
                    doctorUser?.FullName ??
                    string.Empty,

                DocumentType =
                    document.DocumentType
                        .ToString(),

                OriginalFileName =
                    document.OriginalFileName,

                ContentType =
                    document.ContentType,

                FileSize =
                    document.FileSize,

                Description =
                    document.Description,

                IsVerified =
                    document.IsVerified,

                RejectionReason =
                    document.RejectionReason,

                VerifiedByAdminId =
                    document.VerifiedByAdminId,

                VerifiedAt =
                    document.VerifiedAt,

                UploadedAt =
                    document.CreatedAt
            };
        }

        private static void ValidateFile(
            IFormFile file)
        {
            if (file is null ||
                file.Length == 0)
            {
                throw new BadRequestException(
                    "Document file is required.");
            }

            if (file.Length >
                MaxFileSizeInBytes)
            {
                throw new BadRequestException(
                    "Document size must not exceed 10 MB.");
            }

            var extension =
                Path.GetExtension(
                        file.FileName)
                    .ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(
                    extension) ||
                !AllowedExtensions.Contains(
                    extension))
            {
                throw new BadRequestException(
                    "Only PDF, JPG, JPEG, PNG and WEBP files are allowed.");
            }

            var contentType =
                file.ContentType?.Trim();

            if (string.IsNullOrWhiteSpace(
                    contentType) ||
                !AllowedContentTypes.Contains(
                    contentType))
            {
                throw new BadRequestException(
                    "Invalid document content type.");
            }
        }

        private static string?
            NormalizeDescription(
                string? description)
        {
            return string.IsNullOrWhiteSpace(
                description)
                ? null
                : description.Trim();
        }
    }
}
