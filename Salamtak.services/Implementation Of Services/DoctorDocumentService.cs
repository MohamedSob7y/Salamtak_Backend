using AutoMapper;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.DoctorDocuments;
using Salamtak.Shared.Responses;

namespace Salamtak.services.Implementation_Of_Services
{
    public class DoctorDocumentService : IDoctorDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<UploadDoctorDocumentDto> _uploadValidator;
        private readonly IValidator<VerifyDoctorDocumentDto> _verifyValidator;
        private readonly IValidator<RejectDoctorDocumentDto> _rejectValidator;

        public DoctorDocumentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<UploadDoctorDocumentDto> uploadValidator,
            IValidator<VerifyDoctorDocumentDto> verifyValidator,
            IValidator<RejectDoctorDocumentDto> rejectValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _uploadValidator = uploadValidator;
            _verifyValidator = verifyValidator;
            _rejectValidator = rejectValidator;
        }

        public async Task<ApiResponse<DoctorDocumentDto>> UploadAsync(Guid doctorId, UploadDoctorDocumentDto dto)
        {
            var validationResult = await _uploadValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(doctorId);

            if (doctor is null)
                throw new NotFoundException("Doctor not found.");

            if (!Enum.TryParse<DoctorDocumentType>(dto.DocumentType, true, out var documentType))
                throw new BadRequestException("Invalid document type.");

            var sameDocumentExists = await _unitOfWork
                .Repository<DoctorDocument>()
                .AnyAsync(d =>
                    d.DoctorId == doctorId &&
                    d.DocumentType == documentType &&
                    !d.IsDeleted);

            if (sameDocumentExists)
                throw new ConflictException("A document of this type already exists for this doctor.");

            var document = new DoctorDocument
            {
                DoctorId = doctorId,
                DocumentType = documentType,
                FileUrl = dto.FileUrl.Trim(),
                IsVerified = false,
                RejectionReason = null,
                VerifiedAt = null,
                VerifiedByAdminId = null
            };

            doctor.IsVerified = false;
            doctor.VerificationStatus = DoctorVerificationStatus.Pending;

            await _unitOfWork
                .Repository<DoctorDocument>()
                .AddAsync(document);

            _unitOfWork
                .Repository<Doctor>()
                .Update(doctor);

            await _unitOfWork.SaveChangesAsync();

            var result = await BuildDoctorDocumentDtoAsync(document);

            return ApiResponse<DoctorDocumentDto>.Ok(result, "Document uploaded successfully.");
        }

        public async Task<ApiResponse<IReadOnlyList<DoctorDocumentDto>>> GetDoctorDocumentsAsync(Guid doctorId)
        {
            var doctorExists = await _unitOfWork
                .Repository<Doctor>()
                .AnyAsync(d => d.Id == doctorId);

            if (!doctorExists)
                throw new NotFoundException("Doctor not found.");

            var documents = await _unitOfWork
                .Repository<DoctorDocument>()
                .GetAllAsync(d => d.DoctorId == doctorId && !d.IsDeleted);

            var result = new List<DoctorDocumentDto>();

            foreach (var document in documents.OrderByDescending(d => d.CreatedAt))
            {
                result.Add(await BuildDoctorDocumentDtoAsync(document));
            }

            return ApiResponse<IReadOnlyList<DoctorDocumentDto>>.Ok(result);
        }

        public async Task<ApiResponse> VerifyAsync(VerifyDoctorDocumentDto dto)
        {
            var validationResult = await _verifyValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var document = await _unitOfWork
                .Repository<DoctorDocument>()
                .GetByIdAsync(dto.DocumentId);

            if (document is null)
                throw new NotFoundException("Document not found.");

            var adminExists = await _unitOfWork
                .Repository<Admin>()
                .AnyAsync(a => a.Id == dto.AdminId);

            if (!adminExists)
                throw new NotFoundException("Admin not found.");

            document.IsVerified = true;
            document.VerifiedByAdminId = dto.AdminId;
            document.VerifiedAt = DateTime.UtcNow;
            document.RejectionReason = null;

            _unitOfWork
                .Repository<DoctorDocument>()
                .Update(document);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Document verified successfully.");
        }

        public async Task<ApiResponse> RejectAsync(RejectDoctorDocumentDto dto)
        {
            var validationResult = await _rejectValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var document = await _unitOfWork
                .Repository<DoctorDocument>()
                .GetByIdAsync(dto.DocumentId);

            if (document is null)
                throw new NotFoundException("Document not found.");

            var adminExists = await _unitOfWork
                .Repository<Admin>()
                .AnyAsync(a => a.Id == dto.AdminId);

            if (!adminExists)
                throw new NotFoundException("Admin not found.");

            document.IsVerified = false;
            document.VerifiedByAdminId = dto.AdminId;
            document.VerifiedAt = DateTime.UtcNow;
            document.RejectionReason = dto.RejectionReason.Trim();

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(document.DoctorId);

            if (doctor is not null)
            {
                doctor.IsVerified = false;
                doctor.VerificationStatus = DoctorVerificationStatus.Rejected;

                _unitOfWork
                    .Repository<Doctor>()
                    .Update(doctor);
            }

            _unitOfWork
                .Repository<DoctorDocument>()
                .Update(document);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Document rejected successfully.");
        }

        private async Task<DoctorDocumentDto> BuildDoctorDocumentDtoAsync(DoctorDocument document)
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
                DocumentId = document.Id,
                DoctorId = document.DoctorId,
                DoctorName = doctorUser?.FullName ?? string.Empty,
                DocumentType = document.DocumentType.ToString(),
                FileUrl = document.FileUrl,
                IsVerified = document.IsVerified,
                RejectionReason = document.RejectionReason,
                VerifiedByAdminId = document.VerifiedByAdminId,
                VerifiedAt = document.VerifiedAt,
                UploadedAt = document.CreatedAt
            };
        }
    }
}
