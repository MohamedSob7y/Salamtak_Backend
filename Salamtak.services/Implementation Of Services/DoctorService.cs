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

            var doctors = await _unitOfWork
                .Repository<Doctor>()
                .GetAllAsync(d => d.IsVerified);

            var filteredDoctors = doctors.ToList();

            if (dto.SpecialtyId.HasValue)
            {
                filteredDoctors = filteredDoctors
                    .Where(d => d.SpecialtyId == dto.SpecialtyId.Value)
                    .ToList();
            }

            if (dto.MinFee.HasValue)
            {
                filteredDoctors = filteredDoctors
                    .Where(d => d.ConsultationFee.HasValue &&
                                d.ConsultationFee.Value >= dto.MinFee.Value)
                    .ToList();
            }

            if (dto.MaxFee.HasValue)
            {
                filteredDoctors = filteredDoctors
                    .Where(d => d.ConsultationFee.HasValue &&
                                d.ConsultationFee.Value <= dto.MaxFee.Value)
                    .ToList();
            }

            if (dto.MinRating.HasValue)
            {
                filteredDoctors = filteredDoctors
                    .Where(d => d.AverageRating >= dto.MinRating.Value)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(dto.DoctorName))
            {
                var doctorName = dto.DoctorName.Trim().ToLower();

                var matchedUsers = await _unitOfWork
                    .Repository<User>()
                    .GetAllAsync(u => u.FullName.ToLower().Contains(doctorName));

                var matchedUserIds = matchedUsers
                    .Select(u => u.Id)
                    .ToHashSet();

                filteredDoctors = filteredDoctors
                    .Where(d => matchedUserIds.Contains(d.UserId))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(dto.City))
            {
                var city = dto.City.Trim().ToLower();

                var matchedClinics = await _unitOfWork
                    .Repository<Clinic>()
                    .GetAllAsync(c => c.City.ToLower().Contains(city));

                var doctorIdsInCity = matchedClinics
                    .Select(c => c.DoctorId)
                    .Distinct()
                    .ToHashSet();

                filteredDoctors = filteredDoctors
                    .Where(d => doctorIdsInCity.Contains(d.Id))
                    .ToList();
            }

            filteredDoctors = filteredDoctors
                .OrderByDescending(d => d.AverageRating)
                .ThenBy(d => d.ConsultationFee ?? decimal.MaxValue)
                .ToList();

            var totalCount = filteredDoctors.Count;

            var pagedDoctors = filteredDoctors
                .Skip((dto.PageNumber - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToList();

            foreach (var doctor in pagedDoctors)
            {
                await LoadDoctorNavigationDataAsync(doctor);
            }

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
            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(doctorId);

            if (doctor is null)
                throw new NotFoundException("Doctor not found.");

            await LoadDoctorNavigationDataAsync(doctor);
            await LoadDoctorFeedbacksAsync(doctor);

            var result = _mapper.Map<DoctorDetailsDto>(doctor);

            return ApiResponse<DoctorDetailsDto>.Ok(result);
        }

        public async Task<ApiResponse<DoctorProfileDto>> GetProfileAsync(Guid doctorId)
        {
            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(doctorId);

            if (doctor is null)
                throw new NotFoundException("Doctor not found.");

            await LoadDoctorNavigationDataAsync(doctor);

            var result = _mapper.Map<DoctorProfileDto>(doctor);

            return ApiResponse<DoctorProfileDto>.Ok(result);
        }

        public async Task<ApiResponse<DoctorProfileDto>> UpdateProfileAsync(Guid doctorId, UpdateDoctorProfileDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(doctorId);

            if (doctor is null)
                throw new NotFoundException("Doctor not found.");

            var specialty = await _unitOfWork
                .Repository<Specialty>()
                .GetByIdAsync(dto.SpecialtyId);

            if (specialty is null)
                throw new NotFoundException("Specialty not found.");

            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(doctor.UserId);

            if (user is null)
                throw new NotFoundException("User not found.");

            user.FullName = dto.FullName.Trim();
            user.PhoneNumber = dto.PhoneNumber.Trim();

            doctor.SpecialtyId = dto.SpecialtyId;
            doctor.Bio = dto.Bio?.Trim();
            doctor.ConsultationFee = dto.ConsultationFee;
            doctor.ExperienceYears = dto.ExperienceYears;

            _unitOfWork.Repository<User>().Update(user);
            _unitOfWork.Repository<Doctor>().Update(doctor);

            await _unitOfWork.SaveChangesAsync();

            doctor.User = user;
            doctor.Specialty = specialty;

            var result = _mapper.Map<DoctorProfileDto>(doctor);

            return ApiResponse<DoctorProfileDto>.Ok(result, "Doctor profile updated successfully.");
        }

        public async Task<ApiResponse<IReadOnlyList<DoctorAppointmentDto>>> GetAppointmentsAsync(Guid doctorId)
        {
            var doctorExists = await _unitOfWork
                .Repository<Doctor>()
                .AnyAsync(d => d.Id == doctorId);

            if (!doctorExists)
                throw new NotFoundException("Doctor not found.");

            var appointments = await _unitOfWork
                .Repository<Appointment>()
                .GetAllAsync(a => a.DoctorId == doctorId);

            var result = new List<DoctorAppointmentDto>();

            foreach (var appointment in appointments.OrderByDescending(a => a.CreatedAt))
            {
                var patient = await _unitOfWork
                    .Repository<Patient>()
                    .GetByIdAsync(appointment.PatientId);

                var patientUser = patient is null
                    ? null
                    : await _unitOfWork.Repository<User>().GetByIdAsync(patient.UserId);

                var slot = await _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .GetByIdAsync(appointment.AvailabilitySlotId);

                result.Add(new DoctorAppointmentDto
                {
                    AppointmentId = appointment.Id,
                    PatientName = patientUser?.FullName ?? string.Empty,
                    AppointmentDate = slot?.StartTime ?? appointment.CreatedAt,
                    Status = appointment.Status.ToString(),
                    Reason = appointment.Reason ?? string.Empty
                });
            }

            return ApiResponse<IReadOnlyList<DoctorAppointmentDto>>.Ok(result);
        }

        public async Task<ApiResponse<DoctorRatingSummaryDto>> GetRatingSummaryAsync(Guid doctorId)
        {
            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(doctorId);

            if (doctor is null)
                throw new NotFoundException("Doctor not found.");

            var feedbacks = await _unitOfWork
                .Repository<Feedback>()
                .GetAllAsync(f => f.DoctorId == doctorId);

            var result = new DoctorRatingSummaryDto
            {
                DoctorId = doctor.Id,
                AverageRating = feedbacks.Any()
                    ? feedbacks.Average(f => f.Rating)
                    : doctor.AverageRating,
                TotalReviews = feedbacks.Count
            };

            return ApiResponse<DoctorRatingSummaryDto>.Ok(result);
        }

        private async Task LoadDoctorNavigationDataAsync(Doctor doctor)
        {
            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(doctor.UserId);

            var specialty = await _unitOfWork
                .Repository<Specialty>()
                .GetByIdAsync(doctor.SpecialtyId);

            var clinics = await _unitOfWork
                .Repository<Clinic>()
                .GetAllAsync(c => c.DoctorId == doctor.Id);

            if (user is not null)
                doctor.User = user;

            if (specialty is not null)
                doctor.Specialty = specialty;

            doctor.Clinics = clinics.ToList();
        }

        private async Task LoadDoctorFeedbacksAsync(Doctor doctor)
        {
            var feedbacks = await _unitOfWork
                .Repository<Feedback>()
                .GetAllAsync(f => f.DoctorId == doctor.Id);

            doctor.Feedbacks = feedbacks.ToList();
        }
    }
}
