using AutoMapper;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.Appointments;
using Salamtak.Shared.DTOs.Doctors;
using Salamtak.Shared.Responses;

namespace Salamtak.services.Implementation_Of_Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<DoctorSearchRequestDto> _searchValidator;
        private readonly IValidator<UpdateDoctorProfileDto> _updateValidator;

        public DoctorService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<DoctorSearchRequestDto> searchValidator,
            IValidator<UpdateDoctorProfileDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _searchValidator = searchValidator;
            _updateValidator = updateValidator;
        }

        public async Task<ApiResponse<PagedResult<DoctorCardDto>>> SearchDoctorsAsync(DoctorSearchRequestDto dto)
        {
            var validationResult = await _searchValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var doctors = await _unitOfWork.Repository<Doctor>().GetAllAsync(d =>
                d.IsVerified &&
                (!dto.SpecialtyId.HasValue || d.SpecialtyId == dto.SpecialtyId.Value) &&
                (!dto.MinFee.HasValue || d.ConsultationFee >= dto.MinFee.Value) &&
                (!dto.MaxFee.HasValue || d.ConsultationFee <= dto.MaxFee.Value) &&
                (!dto.MinRating.HasValue || d.AverageRating >= dto.MinRating.Value));

            if (!string.IsNullOrWhiteSpace(dto.DoctorName))
                doctors = doctors.Where(d => d.User != null && d.User.FullName.Contains(dto.DoctorName.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(dto.City))
                doctors = doctors.Where(d => d.Clinics.Any(c => c.City.Equals(dto.City.Trim(), StringComparison.OrdinalIgnoreCase))).ToList();

            var totalCount = doctors.Count;

            var pagedDoctors = doctors
                .OrderByDescending(d => d.AverageRating)
                .Skip((dto.PageNumber - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToList();

            var items = _mapper.Map<List<DoctorCardDto>>(pagedDoctors);

            var result = new PagedResult<DoctorCardDto>
            {
                Items = items,
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize,
                TotalCount = totalCount
            };

            return ApiResponse<PagedResult<DoctorCardDto>>.Ok(result);
        }

        public async Task<ApiResponse<DoctorDetailsDto>> GetDoctorDetailsAsync(Guid doctorId)
        {
            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(doctorId);
            if (doctor is null)
                throw new NotFoundException("Doctor not found.");

            var result = _mapper.Map<DoctorDetailsDto>(doctor);
            return ApiResponse<DoctorDetailsDto>.Ok(result);
        }

        public async Task<ApiResponse<DoctorProfileDto>> GetProfileAsync(Guid doctorId)
        {
            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(doctorId);
            if (doctor is null)
                throw new NotFoundException("Doctor not found.");

            var result = _mapper.Map<DoctorProfileDto>(doctor);
            return ApiResponse<DoctorProfileDto>.Ok(result);
        }

        public async Task<ApiResponse<DoctorProfileDto>> UpdateProfileAsync(Guid doctorId, UpdateDoctorProfileDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(doctorId);
            if (doctor is null)
                throw new NotFoundException("Doctor not found.");

            var specialtyExists = await _unitOfWork.Repository<Specialty>().AnyAsync(s => s.Id == dto.SpecialtyId);
            if (!specialtyExists)
                throw new NotFoundException("Specialty not found.");

            doctor.SpecialtyId = dto.SpecialtyId;
            doctor.Bio = dto.Bio?.Trim();
            doctor.ConsultationFee = dto.ConsultationFee;
            doctor.ExperienceYears = dto.ExperienceYears;

            if (doctor.User is not null)
            {
                doctor.User.FullName = dto.FullName.Trim();
                doctor.User.PhoneNumber = dto.PhoneNumber.Trim();
                _unitOfWork.Repository<User>().Update(doctor.User);
            }

            _unitOfWork.Repository<Doctor>().Update(doctor);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<DoctorProfileDto>(doctor);
            return ApiResponse<DoctorProfileDto>.Ok(result, "Doctor profile updated successfully.");
        }

        public async Task<ApiResponse<IReadOnlyList<DoctorAppointmentDto>>> GetAppointmentsAsync(Guid doctorId)
        {
            var doctorExists = await _unitOfWork.Repository<Doctor>().AnyAsync(d => d.Id == doctorId);
            if (!doctorExists)
                throw new NotFoundException("Doctor not found.");

            var appointments = await _unitOfWork.Repository<Appointment>().GetAllAsync(a => a.DoctorId == doctorId);
            var result = _mapper.Map<IReadOnlyList<DoctorAppointmentDto>>(appointments);
            return ApiResponse<IReadOnlyList<DoctorAppointmentDto>>.Ok(result);
        }

        public async Task<ApiResponse<DoctorRatingSummaryDto>> GetRatingSummaryAsync(Guid doctorId)
        {
            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(doctorId);
            if (doctor is null)
                throw new NotFoundException("Doctor not found.");

            var feedbacks = await _unitOfWork.Repository<Feedback>().GetAllAsync(f => f.DoctorId == doctorId);

            var result = new DoctorRatingSummaryDto
            {
                DoctorId = doctor.Id,
                AverageRating = feedbacks.Any() ? feedbacks.Average(f => f.Rating) : doctor.AverageRating,
                TotalReviews = feedbacks.Count
            };

            return ApiResponse<DoctorRatingSummaryDto>.Ok(result);
        }
    }
}
