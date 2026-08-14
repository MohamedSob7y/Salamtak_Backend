using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
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

        private readonly
            IValidator<DoctorSearchRequestDto>
            _searchValidator;

        private readonly
            IValidator<UpdateDoctorProfileDto>
            _updateValidator;

        public DoctorService(
            IUnitOfWork unitOfWork,
            IValidator<DoctorSearchRequestDto>
                searchValidator,
            IValidator<UpdateDoctorProfileDto>
                updateValidator)
        {
            _unitOfWork = unitOfWork;
            _searchValidator = searchValidator;
            _updateValidator = updateValidator;
        }

        // =========================================================
        // Search doctors using specialty and nearest clinic
        // =========================================================
        public async Task<
            ApiResponse<PagedResult<DoctorCardDto>>>
            SearchDoctorsAsync(
                DoctorSearchRequestDto dto)
        {
            var validationResult =
                await _searchValidator
                    .ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors
                        .Select(error =>
                            error.ErrorMessage));
            }

            var doctors = await _unitOfWork
                .Repository<Doctor>()
                .GetAllAsync(doctor =>
                    doctor.IsVerified &&
                    doctor.VerificationStatus ==
                        DoctorVerificationStatus.Verified &&
                    (
                        !dto.SpecialtyId.HasValue ||
                        doctor.SpecialtyId ==
                            dto.SpecialtyId.Value
                    ));

            var doctorList =
                doctors.ToList();

            if (doctorList.Count == 0)
            {
                return CreateEmptySearchResponse(dto);
            }

            var doctorIds = doctorList
                .Select(doctor => doctor.Id)
                .ToList();

            var userIds = doctorList
                .Select(doctor => doctor.UserId)
                .Distinct()
                .ToList();

            var specialtyIds = doctorList
                .Select(doctor => doctor.SpecialtyId)
                .Distinct()
                .ToList();

            var users = await _unitOfWork
                .Repository<User>()
                .GetAllAsync(user =>
                    userIds.Contains(user.Id));

            var specialties = await _unitOfWork
                .Repository<Specialty>()
                .GetAllAsync(specialty =>
                    specialtyIds.Contains(
                        specialty.Id));

            /*
             * العلاقة الجديدة:
             *
             * Doctor
             *   ↓
             * DoctorClinic
             *   ↓
             * Clinic
             */
            var doctorClinicRelations =
                await _unitOfWork
                    .Repository<DoctorClinic>()
                    .GetAllAsync(relation =>
                        doctorIds.Contains(
                            relation.DoctorId));

            var relationList =
                doctorClinicRelations.ToList();

            var clinicIds = relationList
                .Select(relation =>
                    relation.ClinicId)
                .Distinct()
                .ToList();

            var clinics =
                clinicIds.Count == 0
                    ? new List<Clinic>()
                    : (await _unitOfWork
                        .Repository<Clinic>()
                        .GetAllAsync(clinic =>
                            clinicIds.Contains(
                                clinic.Id)))
                        .ToList();

            var feedbacks = await _unitOfWork
                .Repository<Feedback>()
                .GetAllAsync(feedback =>
                    doctorIds.Contains(
                        feedback.DoctorId));

            var userDictionary =
                users.ToDictionary(
                    user => user.Id);

            var specialtyDictionary =
                specialties.ToDictionary(
                    specialty => specialty.Id);

            var clinicDictionary =
                clinics.ToDictionary(
                    clinic => clinic.Id);

            var feedbackDictionary =
                feedbacks
                    .GroupBy(feedback =>
                        feedback.DoctorId)
                    .ToDictionary(
                        group => group.Key,
                        group => group.ToList());

            var searchResults =
                new List<DoctorCardDto>();

            foreach (var doctor in doctorList)
            {
                userDictionary.TryGetValue(
                    doctor.UserId,
                    out var user);

                specialtyDictionary.TryGetValue(
                    doctor.SpecialtyId,
                    out var specialty);

                feedbackDictionary.TryGetValue(
                    doctor.Id,
                    out var doctorFeedbacks);

                doctorFeedbacks ??=
                    new List<Feedback>();

                var relatedClinicIds =
                    relationList
                        .Where(relation =>
                            relation.DoctorId ==
                                doctor.Id)
                        .Select(relation =>
                            relation.ClinicId)
                        .Distinct()
                        .ToList();

                var doctorClinics =
                    relatedClinicIds
                        .Where(clinicDictionary
                            .ContainsKey)
                        .Select(clinicId =>
                            clinicDictionary[
                                clinicId])
                        .ToList();

                /*
                 * الدكتور بدون Clinic لا يظهر في البحث،
                 * لأنه لا يوجد مكان يمكن للمريض الحجز فيه.
                 */
                if (doctorClinics.Count == 0)
                {
                    continue;
                }

                Clinic nearestClinic;
                double? distanceKm = null;

                var locationWasProvided =
                    dto.Latitude.HasValue &&
                    dto.Longitude.HasValue;

                if (locationWasProvided)
                {
                    var clinicsWithCoordinates =
                        doctorClinics
                            .Where(clinic =>
                                clinic.Latitude
                                    .HasValue &&
                                clinic.Longitude
                                    .HasValue)
                            .Select(clinic =>
                                new
                                {
                                    Clinic = clinic,

                                    Distance =
                                        CalculateDistanceKm(
                                            dto.Latitude!
                                                .Value,

                                            dto.Longitude!
                                                .Value,

                                            clinic.Latitude!
                                                .Value,

                                            clinic.Longitude!
                                                .Value)
                                })
                            .OrderBy(item =>
                                item.Distance)
                            .ToList();

                    if (clinicsWithCoordinates.Count ==
                        0)
                    {
                        continue;
                    }

                    var nearest =
                        clinicsWithCoordinates[0];

                    nearestClinic =
                        nearest.Clinic;

                    distanceKm =
                        nearest.Distance;

                    if (dto.MaxDistanceKm.HasValue &&
                        distanceKm >
                        dto.MaxDistanceKm.Value)
                    {
                        continue;
                    }
                }
                else
                {
                    nearestClinic =
                        doctorClinics
                            .OrderBy(clinic =>
                                clinic.Name)
                            .First();
                }

                var averageRating =
                    doctorFeedbacks.Count > 0
                        ? doctorFeedbacks.Average(
                            feedback =>
                                Convert.ToDouble(
                                    feedback.Rating))
                        : doctor.AverageRating;

                searchResults.Add(
                    new DoctorCardDto
                    {
                        DoctorId =
                            doctor.Id,

                        SpecialtyId =
                            doctor.SpecialtyId,

                        FullName =
                            user?.FullName ??
                            string.Empty,

                        SpecialtyName =
                            specialty?.Name ??
                            string.Empty,

                        NearestClinicId =
                            nearestClinic.Id,

                        NearestClinicName =
                            nearestClinic.Name,

                        City =
                            nearestClinic.City,

                        ClinicAddress =
                            nearestClinic.Address,

                        DistanceKm =
                            distanceKm.HasValue
                                ? Math.Round(
                                    distanceKm.Value,
                                    2)
                                : null,

                        AverageRating =
                            Math.Round(
                                averageRating,
                                2),

                        ReviewsCount =
                            doctorFeedbacks.Count,

                        ConsultationFee =
                            doctor.ConsultationFee ??
                            0,

                        IsVerified =
                            doctor.IsVerified
                    });
            }

            var orderedResults =
                searchResults
                    .OrderBy(result =>
                        result.DistanceKm ??
                        double.MaxValue)
                    .ThenByDescending(result =>
                        result.AverageRating)
                    .ToList();

            var totalCount =
                orderedResults.Count;

            var items =
                orderedResults
                    .Skip(
                        (dto.PageNumber - 1) *
                        dto.PageSize)
                    .Take(dto.PageSize)
                    .ToList();

            var pagedResult =
                new PagedResult<DoctorCardDto>
                {
                    Items = items,
                    PageNumber =
                        dto.PageNumber,
                    PageSize =
                        dto.PageSize,
                    TotalCount =
                        totalCount
                };

            return ApiResponse<
                PagedResult<DoctorCardDto>>.Ok(
                    pagedResult,
                    "Doctors retrieved successfully.");
        }

        // =========================================================
        // Public doctor details
        // =========================================================
        public async Task<ApiResponse<DoctorDetailsDto>>
            GetDoctorDetailsAsync(
                Guid doctorId)
        {
            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(item =>
                    item.Id == doctorId &&
                    item.IsVerified &&
                    item.VerificationStatus ==
                        DoctorVerificationStatus.Verified);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Verified doctor not found.");
            }

            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(
                    doctor.UserId);

            var specialty = await _unitOfWork
                .Repository<Specialty>()
                .GetByIdAsync(
                    doctor.SpecialtyId);

            if (user is null)
            {
                throw new NotFoundException(
                    "Doctor user account not found.");
            }

            if (specialty is null)
            {
                throw new NotFoundException(
                    "Doctor specialty not found.");
            }

            var feedbacks = await _unitOfWork
                .Repository<Feedback>()
                .GetAllAsync(feedback =>
                    feedback.DoctorId ==
                    doctor.Id);

            var feedbackList =
                feedbacks.ToList();

            var averageRating =
                feedbackList.Count > 0
                    ? feedbackList.Average(
                        feedback =>
                            Convert.ToDouble(
                                feedback.Rating))
                    : doctor.AverageRating;

            var result =
                new DoctorDetailsDto
                {
                    DoctorId =
                        doctor.Id,

                    UserId =
                        doctor.UserId,

                    FullName =
                        user.FullName,

                    Email =
                        user.Email,

                    PhoneNumber =
                        user.PhoneNumber ??
                        string.Empty,

                    SpecialtyId =
                        doctor.SpecialtyId,

                    SpecialtyName =
                        specialty.Name,

                    Bio =
                        doctor.Bio,

                    ExperienceYears =
                        doctor.ExperienceYears,

                    LicenseNumber =
                        doctor.LicenseNumber,

                    VerificationStatus =
                        doctor.VerificationStatus
                            .ToString(),

                    IsVerified =
                        doctor.IsVerified,

                    AverageRating =
                        Math.Round(
                            averageRating,
                            2),

                    ReviewsCount =
                        feedbackList.Count,

                    ConsultationFee =
                        doctor.ConsultationFee ??
                        0
                };

            return ApiResponse<DoctorDetailsDto>.Ok(
                result,
                "Doctor details retrieved successfully.");
        }

        // =========================================================
        // Logged-in doctor profile
        // =========================================================
        public async Task<ApiResponse<DoctorProfileDto>>
            GetProfileAsync(
                Guid doctorUserId)
        {
            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(
                    doctor.UserId);

            var specialty = await _unitOfWork
                .Repository<Specialty>()
                .GetByIdAsync(
                    doctor.SpecialtyId);

            if (user is null)
            {
                throw new NotFoundException(
                    "User account not found.");
            }

            if (specialty is null)
            {
                throw new NotFoundException(
                    "Specialty not found.");
            }

            var result =
                new DoctorProfileDto
                {
                    DoctorId =
                        doctor.Id,

                    UserId =
                        doctor.UserId,

                    FullName =
                        user.FullName,

                    Email =
                        user.Email,

                    PhoneNumber =
                        user.PhoneNumber ??
                        string.Empty,

                    SpecialtyId =
                        doctor.SpecialtyId,

                    SpecialtyName =
                        specialty.Name,

                    Bio =
                        doctor.Bio,

                    LicenseNumber =
                        doctor.LicenseNumber,

                    ConsultationFee =
                        doctor.ConsultationFee ??
                        0,

                    IsVerified =
                        doctor.IsVerified,

                    ExperienceYears =
                        doctor.ExperienceYears
                };

            return ApiResponse<DoctorProfileDto>.Ok(
                result,
                "Doctor profile retrieved successfully.");
        }

        // =========================================================
        // Update logged-in doctor profile
        // =========================================================
        public async Task<ApiResponse<DoctorProfileDto>>
            UpdateProfileAsync(
                Guid doctorUserId,
                UpdateDoctorProfileDto dto)
        {
            var validationResult =
                await _updateValidator
                    .ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors
                        .Select(error =>
                            error.ErrorMessage));
            }

            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(
                    doctor.UserId);

            if (user is null)
            {
                throw new NotFoundException(
                    "User account not found.");
            }

            var specialty = await _unitOfWork
                .Repository<Specialty>()
                .GetByIdAsync(
                    dto.SpecialtyId);

            if (specialty is null)
            {
                throw new NotFoundException(
                    "Specialty not found.");
            }

            var normalizedPhone =
                dto.PhoneNumber.Trim();

            var phoneExists = await _unitOfWork
                .Repository<User>()
                .AnyAsync(existingUser =>
                    existingUser.Id != user.Id &&
                    existingUser.PhoneNumber ==
                        normalizedPhone);

            if (phoneExists)
            {
                throw new ConflictException(
                    "Phone number is already used by another user.");
            }

            user.FullName =
                dto.FullName.Trim();

            user.PhoneNumber =
                normalizedPhone;

            doctor.SpecialtyId =
                dto.SpecialtyId;

            doctor.Bio =
                string.IsNullOrWhiteSpace(
                    dto.Bio)
                    ? null
                    : dto.Bio.Trim();

            doctor.ConsultationFee =
                dto.ConsultationFee;

            doctor.ExperienceYears =
                dto.ExperienceYears;

            _unitOfWork
                .Repository<User>()
                .Update(user);

            _unitOfWork
                .Repository<Doctor>()
                .Update(doctor);

            await _unitOfWork.SaveChangesAsync();

            var result =
                new DoctorProfileDto
                {
                    DoctorId =
                        doctor.Id,

                    UserId =
                        user.Id,

                    FullName =
                        user.FullName,

                    Email =
                        user.Email,

                    PhoneNumber =
                        user.PhoneNumber ??
                        string.Empty,

                    SpecialtyId =
                        doctor.SpecialtyId,

                    SpecialtyName =
                        specialty.Name,

                    Bio =
                        doctor.Bio,

                    LicenseNumber =
                        doctor.LicenseNumber,

                    ConsultationFee =
                        doctor.ConsultationFee ??
                        0,

                    IsVerified =
                        doctor.IsVerified,

                    ExperienceYears =
                        doctor.ExperienceYears
                };

            return ApiResponse<DoctorProfileDto>.Ok(
                result,
                "Doctor profile updated successfully.");
        }

        // =========================================================
        // Doctor appointments
        // =========================================================
        public async Task<
            ApiResponse<
                IReadOnlyList<DoctorAppointmentDto>>>
            GetAppointmentsAsync(
                Guid doctorUserId)
        {
            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            var appointments = await _unitOfWork
                .Repository<Appointment>()
                .GetAllAsync(appointment =>
                    appointment.DoctorId ==
                    doctor.Id);

            var appointmentList =
                appointments
                    .OrderByDescending(
                        appointment =>
                            appointment.CreatedAt)
                    .ToList();

            var result =
                new List<DoctorAppointmentDto>();

            foreach (var appointment in
                     appointmentList)
            {
                var patient = await _unitOfWork
                    .Repository<Patient>()
                    .GetByIdAsync(
                        appointment.PatientId);

                var patientUser =
                    patient is null
                        ? null
                        : await _unitOfWork
                            .Repository<User>()
                            .GetByIdAsync(
                                patient.UserId);

                var slot = await _unitOfWork
                    .Repository<AvailabilitySlot>()
                    .GetByIdAsync(
                        appointment
                            .AvailabilitySlotId);

                result.Add(
                    new DoctorAppointmentDto
                    {
                        AppointmentId =
                            appointment.Id,

                        PatientName =
                            patientUser?.FullName ??
                            string.Empty,

                        AppointmentDate =
                            slot?.StartTime ??
                            appointment.CreatedAt,

                        Status =
                            appointment.Status
                                .ToString(),

                        Reason =
                            appointment.Reason ??
                            string.Empty
                    });
            }

            return ApiResponse<
                IReadOnlyList<
                    DoctorAppointmentDto>>.Ok(
                        result,
                        "Doctor appointments retrieved successfully.");
        }

        // =========================================================
        // Doctor rating summary
        // =========================================================
        public async Task<
            ApiResponse<DoctorRatingSummaryDto>>
            GetRatingSummaryAsync(
                Guid doctorUserId)
        {
            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            var feedbacks = await _unitOfWork
                .Repository<Feedback>()
                .GetAllAsync(feedback =>
                    feedback.DoctorId ==
                    doctor.Id);

            var feedbackList =
                feedbacks.ToList();

            var averageRating =
                feedbackList.Count > 0
                    ? feedbackList.Average(
                        feedback =>
                            Convert.ToDouble(
                                feedback.Rating))
                    : doctor.AverageRating;

            var result =
                new DoctorRatingSummaryDto
                {
                    DoctorId =
                        doctor.Id,

                    AverageRating =
                        Math.Round(
                            averageRating,
                            2),

                    TotalReviews =
                        feedbackList.Count
                };

            return ApiResponse<
                DoctorRatingSummaryDto>.Ok(
                    result,
                    "Doctor rating summary retrieved successfully.");
        }

        // =========================================================
        // Helpers
        // =========================================================
        private async Task<Doctor>
            GetDoctorByUserIdAsync(
                Guid doctorUserId)
        {
            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(item =>
                    item.UserId ==
                    doctorUserId);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor profile not found.");
            }

            return doctor;
        }

        private static ApiResponse<
            PagedResult<DoctorCardDto>>
            CreateEmptySearchResponse(
                DoctorSearchRequestDto dto)
        {
            var result =
                new PagedResult<DoctorCardDto>
                {
                    Items =
                        new List<DoctorCardDto>(),

                    PageNumber =
                        dto.PageNumber,

                    PageSize =
                        dto.PageSize,

                    TotalCount = 0
                };

            return ApiResponse<
                PagedResult<DoctorCardDto>>.Ok(
                    result,
                    "No doctors found.");
        }

        private static double
            CalculateDistanceKm(
                double latitude1,
                double longitude1,
                double latitude2,
                double longitude2)
        {
            const double earthRadiusKm =
                6371;

            var latitudeDifference =
                DegreesToRadians(
                    latitude2 -
                    latitude1);

            var longitudeDifference =
                DegreesToRadians(
                    longitude2 -
                    longitude1);

            var firstLatitude =
                DegreesToRadians(
                    latitude1);

            var secondLatitude =
                DegreesToRadians(
                    latitude2);

            var value =
                Math.Pow(
                    Math.Sin(
                        latitudeDifference / 2),
                    2)
                +
                Math.Cos(firstLatitude)
                *
                Math.Cos(secondLatitude)
                *
                Math.Pow(
                    Math.Sin(
                        longitudeDifference / 2),
                    2);

            value =
                Math.Clamp(
                    value,
                    0,
                    1);

            var angularDistance =
                2 *
                Math.Atan2(
                    Math.Sqrt(value),
                    Math.Sqrt(1 - value));

            return earthRadiusKm *
                   angularDistance;
        }

        private static double
            DegreesToRadians(
                double degrees)
        {
            return degrees *
                   Math.PI /
                   180;
        }
    }
}