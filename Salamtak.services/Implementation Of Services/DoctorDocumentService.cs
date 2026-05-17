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

        public async Task<ApiResponse<DoctorDocumentDto>> UploadAsync(UploadDoctorDocumentDto dto)
        {
            var validationResult = await _uploadValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(dto.DoctorId);
            if (doctor is null)
                throw new NotFoundException("Doctor not found.");

            var document = new DoctorDocument
            {
                DoctorId = dto.DoctorId,
                DocumentType = Enum.Parse<DoctorDocumentType>(dto.DocumentType, true),
                FileUrl = dto.FileUrl.Trim(),
                IsVerified = false
            };

            await _unitOfWork.Repository<DoctorDocument>().AddAsync(document);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<DoctorDocumentDto>(document);
            return ApiResponse<DoctorDocumentDto>.Ok(result, "Document uploaded successfully.");
        }

        public async Task<ApiResponse<IReadOnlyList<DoctorDocumentDto>>> GetDoctorDocumentsAsync(Guid doctorId)
        {
            var doctorExists = await _unitOfWork.Repository<Doctor>().AnyAsync(d => d.Id == doctorId);
            if (!doctorExists)
                throw new NotFoundException("Doctor not found.");

            var documents = await _unitOfWork.Repository<DoctorDocument>().GetAllAsync(d => d.DoctorId == doctorId);
            var result = _mapper.Map<IReadOnlyList<DoctorDocumentDto>>(documents);

            return ApiResponse<IReadOnlyList<DoctorDocumentDto>>.Ok(result);
        }

        public async Task<ApiResponse> VerifyAsync(VerifyDoctorDocumentDto dto)
        {
            var validationResult = await _verifyValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var document = await _unitOfWork.Repository<DoctorDocument>().GetByIdAsync(dto.DocumentId);
            if (document is null)
                throw new NotFoundException("Document not found.");

            var adminExists = await _unitOfWork.Repository<Admin>().AnyAsync(a => a.Id == dto.AdminId);
            if (!adminExists)
                throw new NotFoundException("Admin not found.");

            document.IsVerified = true;
            document.VerifiedByAdminId = dto.AdminId;
            document.VerifiedAt = DateTime.UtcNow;
            document.RejectionReason = null;

            _unitOfWork.Repository<DoctorDocument>().Update(document);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Document verified successfully.");
        }

        public async Task<ApiResponse> RejectAsync(RejectDoctorDocumentDto dto)
        {
            var validationResult = await _rejectValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var document = await _unitOfWork.Repository<DoctorDocument>().GetByIdAsync(dto.DocumentId);
            if (document is null)
                throw new NotFoundException("Document not found.");

            var adminExists = await _unitOfWork.Repository<Admin>().AnyAsync(a => a.Id == dto.AdminId);
            if (!adminExists)
                throw new NotFoundException("Admin not found.");

            document.IsVerified = false;
            document.VerifiedByAdminId = dto.AdminId;
            document.VerifiedAt = DateTime.UtcNow;
            document.RejectionReason = dto.RejectionReason.Trim();

            _unitOfWork.Repository<DoctorDocument>().Update(document);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Document rejected successfully.");
        }
    }
}
