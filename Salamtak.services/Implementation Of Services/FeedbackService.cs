using AutoMapper;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.Feedbacks;
using Salamtak.Shared.Responses;

namespace Salamtak.services.Implementation_Of_Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IValidator<CreateFeedbackDto>_createValidator;

        private readonly IValidator<UpdateFeedbackDto> _updateValidator;

        public FeedbackService(IUnitOfWork unitOfWork,IValidator<CreateFeedbackDto> createValidator,IValidator<UpdateFeedbackDto> updateValidator)
        {
            _unitOfWork =
                unitOfWork ??
                throw new ArgumentNullException(
                    nameof(unitOfWork));

            _createValidator =
                createValidator ??
                throw new ArgumentNullException(
                    nameof(createValidator));

            _updateValidator =
                updateValidator ??
                throw new ArgumentNullException(
                    nameof(updateValidator));
        }

        public async Task<ApiResponse<FeedbackDto>>
            CreateAsync(
                Guid patientUserId,
                CreateFeedbackDto dto,
                CancellationToken cancellationToken = default)
        {
            var validationResult =
                await _createValidator.ValidateAsync(
                    dto,
                    cancellationToken);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors
                        .Select(error =>
                            error.ErrorMessage));
            }

            var patient =
                await GetPatientByUserIdAsync(
                    patientUserId);

            var appointment = await _unitOfWork
                .Repository<Appointment>()
                .GetByIdAsync(dto.AppointmentId);

            if (appointment is null ||
                appointment.IsDeleted)
            {
                throw new NotFoundException(
                    "Appointment not found.");
            }

            if (appointment.PatientId != patient.Id)
            {
                throw new ForbiddenException(
                    "You cannot review this appointment.");
            }

            if (appointment.Status !=
                AppointmentStatus.Completed)
            {
                throw new BadRequestException(
                    "Only completed appointments can be reviewed.");
            }

            var feedbackExists = await _unitOfWork
                .Repository<Feedback>()
                .AnyAsync(feedback =>
                    feedback.AppointmentId ==
                    appointment.Id);

            if (feedbackExists)
            {
                throw new ConflictException(
                    "Feedback already exists for this appointment.");
            }

            var doctorExists = await _unitOfWork
                .Repository<Doctor>()
                .AnyAsync(doctor =>
                    doctor.Id == appointment.DoctorId &&
                    !doctor.IsDeleted);

            if (!doctorExists)
            {
                throw new NotFoundException(
                    "Doctor not found.");
            }

            var feedback = new Feedback
            {
                PatientId =
                    patient.Id,

                DoctorId =
                    appointment.DoctorId,

                AppointmentId =
                    appointment.Id,

                Rating =
                    dto.Rating,

                Comment =
                    NormalizeComment(dto.Comment)
            };

            await _unitOfWork
                .Repository<Feedback>()
                .AddAsync(feedback);

            await _unitOfWork.SaveChangesAsync();

            await RefreshDoctorRatingAsync(
                feedback.DoctorId);

            await _unitOfWork.SaveChangesAsync();

            var result =
                await BuildFeedbackDtoAsync(
                    feedback);

            return ApiResponse<FeedbackDto>.Ok(
                result,
                "Feedback created successfully.");
        }

        public async Task<ApiResponse<FeedbackDto>>
            UpdateAsync(
                Guid patientUserId,
                UpdateFeedbackDto dto,
                CancellationToken cancellationToken = default)
        {
            var validationResult =
                await _updateValidator.ValidateAsync(
                    dto,
                    cancellationToken);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors
                        .Select(error =>
                            error.ErrorMessage));
            }

            var patient =
                await GetPatientByUserIdAsync(
                    patientUserId);

            var feedback = await _unitOfWork
                .Repository<Feedback>()
                .GetByIdAsync(dto.FeedbackId);

            if (feedback is null ||
                feedback.IsDeleted)
            {
                throw new NotFoundException(
                    "Feedback not found.");
            }

            if (feedback.PatientId != patient.Id)
            {
                throw new ForbiddenException(
                    "You cannot update this feedback.");
            }

            feedback.Rating =
                dto.Rating;

            feedback.Comment =
                NormalizeComment(dto.Comment);

            _unitOfWork
                .Repository<Feedback>()
                .Update(feedback);

            await _unitOfWork.SaveChangesAsync();

            await RefreshDoctorRatingAsync(
                feedback.DoctorId);

            await _unitOfWork.SaveChangesAsync();

            var result =
                await BuildFeedbackDtoAsync(
                    feedback);

            return ApiResponse<FeedbackDto>.Ok(
                result,
                "Feedback updated successfully.");
        }

        public async Task<
            ApiResponse<IReadOnlyList<DoctorFeedbackDto>>>
            GetDoctorFeedbacksAsync(
                Guid doctorId,
                CancellationToken cancellationToken = default)
        {
            var doctorExists = await _unitOfWork
                .Repository<Doctor>()
                .AnyAsync(doctor =>
                    doctor.Id == doctorId &&
                    !doctor.IsDeleted);

            if (!doctorExists)
            {
                throw new NotFoundException(
                    "Doctor not found.");
            }

            var feedbacks = await _unitOfWork
                .Repository<Feedback>()
                .GetAllAsync(feedback =>
                    feedback.DoctorId == doctorId &&
                    !feedback.IsDeleted);

            var orderedFeedbacks = feedbacks
                .OrderByDescending(feedback =>
                    feedback.CreatedAt)
                .ToList();

            var patientIds = orderedFeedbacks
                .Select(feedback =>
                    feedback.PatientId)
                .Distinct()
                .ToList();

            var patients = await _unitOfWork
                .Repository<Patient>()
                .GetAllAsync(patient =>
                    patientIds.Contains(patient.Id) &&
                    !patient.IsDeleted);

            var patientList =
                patients.ToList();

            var userIds = patientList
                .Select(patient =>
                    patient.UserId)
                .Distinct()
                .ToList();

            var users = await _unitOfWork
                .Repository<User>()
                .GetAllAsync(user =>
                    userIds.Contains(user.Id) &&
                    !user.IsDeleted);

            var userLookup = users
                .ToDictionary(
                    user => user.Id,
                    user => user);

            var patientUserIdLookup = patientList
                .ToDictionary(
                    patient => patient.Id,
                    patient => patient.UserId);

            var result =
                new List<DoctorFeedbackDto>();

            foreach (var feedback in orderedFeedbacks)
            {
                string patientName =
                    string.Empty;

                if (patientUserIdLookup.TryGetValue(
                        feedback.PatientId,
                        out var patientUserId) &&
                    userLookup.TryGetValue(
                        patientUserId,
                        out var patientUser))
                {
                    patientName =
                        patientUser.FullName;
                }

                result.Add(
                    new DoctorFeedbackDto
                    {
                        FeedbackId =
                            feedback.Id,

                        PatientName =
                            patientName,

                        Rating =
                            feedback.Rating,

                        Comment =
                            feedback.Comment ??
                            string.Empty,

                        CreatedAt =
                            feedback.CreatedAt
                    });
            }

            return ApiResponse<
                IReadOnlyList<DoctorFeedbackDto>>.Ok(
                    result,
                    "Doctor feedbacks retrieved successfully.");
        }

        public async Task<ApiResponse>
            DeleteAsync(
                Guid patientUserId,
                Guid feedbackId,
                CancellationToken cancellationToken = default)
        {
            var patient =
                await GetPatientByUserIdAsync(
                    patientUserId);

            var feedback = await _unitOfWork
                .Repository<Feedback>()
                .GetByIdAsync(feedbackId);

            if (feedback is null ||
                feedback.IsDeleted)
            {
                throw new NotFoundException(
                    "Feedback not found.");
            }

            if (feedback.PatientId != patient.Id)
            {
                throw new ForbiddenException(
                    "You cannot delete this feedback.");
            }

            var doctorId =
                feedback.DoctorId;

            _unitOfWork
                .Repository<Feedback>()
                .SoftDelete(feedback);

            await _unitOfWork.SaveChangesAsync();

            await RefreshDoctorRatingAsync(
                doctorId);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok(
                "Feedback deleted successfully.");
        }

        private async Task<Patient>
            GetPatientByUserIdAsync(
                Guid patientUserId)
        {
            var patient = await _unitOfWork
                .Repository<Patient>()
                .FirstOrDefaultAsync(patient =>
                    patient.UserId == patientUserId &&
                    !patient.IsDeleted);

            if (patient is null)
            {
                throw new NotFoundException(
                    "Patient profile not found.");
            }

            return patient;
        }

        private async Task
            RefreshDoctorRatingAsync(
                Guid doctorId)
        {
            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(doctorId);

            if (doctor is null ||
                doctor.IsDeleted)
            {
                throw new NotFoundException(
                    "Doctor not found.");
            }

            var feedbacks = await _unitOfWork
                .Repository<Feedback>()
                .GetAllAsync(feedback =>
                    feedback.DoctorId == doctorId &&
                    !feedback.IsDeleted);

            doctor.AverageRating =
                feedbacks.Any()
                    ? feedbacks.Average(
                        feedback =>
                            feedback.Rating)
                    : 0;

            _unitOfWork
                .Repository<Doctor>()
                .Update(doctor);
        }

        private async Task<FeedbackDto>
            BuildFeedbackDtoAsync(
                Feedback feedback)
        {
            var patient = await _unitOfWork
                .Repository<Patient>()
                .GetByIdAsync(feedback.PatientId);

            User? patientUser =
                null;

            if (patient is not null)
            {
                patientUser = await _unitOfWork
                    .Repository<User>()
                    .GetByIdAsync(patient.UserId);
            }

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(feedback.DoctorId);

            User? doctorUser =
                null;

            if (doctor is not null)
            {
                doctorUser = await _unitOfWork
                    .Repository<User>()
                    .GetByIdAsync(doctor.UserId);
            }

            return new FeedbackDto
            {
                FeedbackId =
                    feedback.Id,

                PatientId =
                    feedback.PatientId,

                PatientName =
                    patientUser?.FullName ??
                    string.Empty,

                DoctorId =
                    feedback.DoctorId,

                DoctorName =
                    doctorUser?.FullName ??
                    string.Empty,

                AppointmentId =
                    feedback.AppointmentId,

                Rating =
                    feedback.Rating,

                Comment =
                    feedback.Comment ??
                    string.Empty,

                CreatedAt =
                    feedback.CreatedAt
            };
        }

        private static string? NormalizeComment(
            string? comment)
        {
            return string.IsNullOrWhiteSpace(comment)
                ? null
                : comment.Trim();
        }
    }
}
