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
        private readonly IMapper _mapper;
        private readonly IValidator<CreateFeedbackDto> _createValidator;
        private readonly IValidator<UpdateFeedbackDto> _updateValidator;

        public FeedbackService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateFeedbackDto> createValidator,
            IValidator<UpdateFeedbackDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<ApiResponse<FeedbackDto>> CreateAsync(Guid patientId, CreateFeedbackDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var patientExists = await _unitOfWork
                .Repository<Patient>()
                .AnyAsync(p => p.Id == patientId);

            if (!patientExists)
                throw new NotFoundException("Patient not found.");

            var appointment = await _unitOfWork
                .Repository<Appointment>()
                .GetByIdAsync(dto.AppointmentId);

            if (appointment is null)
                throw new NotFoundException("Appointment not found.");

            if (appointment.PatientId != patientId)
                throw new ForbiddenException("You cannot review this appointment.");

            if (appointment.Status != AppointmentStatus.Completed)
                throw new BadRequestException("Only completed appointments can be reviewed.");

            var feedbackExists = await _unitOfWork
                .Repository<Feedback>()
                .AnyAsync(f => f.AppointmentId == dto.AppointmentId);

            if (feedbackExists)
                throw new ConflictException("Feedback already exists for this appointment.");

            var feedback = new Feedback
            {
                PatientId = patientId,
                DoctorId = appointment.DoctorId,
                AppointmentId = appointment.Id,
                Rating = dto.Rating,
                Comment = dto.Comment?.Trim()
            };

            await _unitOfWork
                .Repository<Feedback>()
                .AddAsync(feedback);

            await _unitOfWork.SaveChangesAsync();

            await RefreshDoctorRatingAsync(appointment.DoctorId);

            await _unitOfWork.SaveChangesAsync();

            var result = await BuildFeedbackDtoAsync(feedback);

            return ApiResponse<FeedbackDto>.Ok(result, "Feedback created successfully.");
        }

        public async Task<ApiResponse<FeedbackDto>> UpdateAsync(Guid patientId, UpdateFeedbackDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var feedback = await _unitOfWork
                .Repository<Feedback>()
                .GetByIdAsync(dto.FeedbackId);

            if (feedback is null)
                throw new NotFoundException("Feedback not found.");

            if (feedback.PatientId != patientId)
                throw new ForbiddenException("You cannot update this feedback.");

            feedback.Rating = dto.Rating;
            feedback.Comment = dto.Comment?.Trim();

            _unitOfWork
                .Repository<Feedback>()
                .Update(feedback);

            await _unitOfWork.SaveChangesAsync();

            await RefreshDoctorRatingAsync(feedback.DoctorId);

            await _unitOfWork.SaveChangesAsync();

            var result = await BuildFeedbackDtoAsync(feedback);

            return ApiResponse<FeedbackDto>.Ok(result, "Feedback updated successfully.");
        }

        public async Task<ApiResponse<IReadOnlyList<DoctorFeedbackDto>>> GetDoctorFeedbacksAsync(Guid doctorId)
        {
            var doctorExists = await _unitOfWork
                .Repository<Doctor>()
                .AnyAsync(d => d.Id == doctorId);

            if (!doctorExists)
                throw new NotFoundException("Doctor not found.");

            var feedbacks = await _unitOfWork
                .Repository<Feedback>()
                .GetAllAsync(f => f.DoctorId == doctorId && !f.IsDeleted);

            var result = new List<DoctorFeedbackDto>();

            foreach (var feedback in feedbacks.OrderByDescending(f => f.CreatedAt))
            {
                var patient = await _unitOfWork
                    .Repository<Patient>()
                    .GetByIdAsync(feedback.PatientId);

                User? patientUser = null;

                if (patient is not null)
                {
                    patientUser = await _unitOfWork
                        .Repository<User>()
                        .GetByIdAsync(patient.UserId);
                }

                result.Add(new DoctorFeedbackDto
                {
                    FeedbackId = feedback.Id,
                    PatientName = patientUser?.FullName ?? string.Empty,
                    Rating = feedback.Rating,
                    Comment = feedback.Comment ?? string.Empty,
                    CreatedAt = feedback.CreatedAt
                });
            }

            return ApiResponse<IReadOnlyList<DoctorFeedbackDto>>.Ok(result);
        }

        public async Task<ApiResponse> DeleteAsync(Guid patientId, Guid feedbackId)
        {
            var feedback = await _unitOfWork
                .Repository<Feedback>()
                .GetByIdAsync(feedbackId);

            if (feedback is null)
                throw new NotFoundException("Feedback not found.");

            if (feedback.PatientId != patientId)
                throw new ForbiddenException("You cannot delete this feedback.");

            var doctorId = feedback.DoctorId;

            _unitOfWork
                .Repository<Feedback>()
                .SoftDelete(feedback);

            await _unitOfWork.SaveChangesAsync();

            await RefreshDoctorRatingAsync(doctorId);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Feedback deleted successfully.");
        }

        private async Task RefreshDoctorRatingAsync(Guid doctorId)
        {
            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(doctorId);

            if (doctor is null)
                return;

            var feedbacks = await _unitOfWork
                .Repository<Feedback>()
                .GetAllAsync(f => f.DoctorId == doctorId && !f.IsDeleted);

            doctor.AverageRating = feedbacks.Any()
                ? feedbacks.Average(f => f.Rating)
                : 0;

            _unitOfWork
                .Repository<Doctor>()
                .Update(doctor);
        }

        private async Task<FeedbackDto> BuildFeedbackDtoAsync(Feedback feedback)
        {
            var patient = await _unitOfWork
                .Repository<Patient>()
                .GetByIdAsync(feedback.PatientId);

            User? patientUser = null;

            if (patient is not null)
            {
                patientUser = await _unitOfWork
                    .Repository<User>()
                    .GetByIdAsync(patient.UserId);
            }

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(feedback.DoctorId);

            User? doctorUser = null;

            if (doctor is not null)
            {
                doctorUser = await _unitOfWork
                    .Repository<User>()
                    .GetByIdAsync(doctor.UserId);
            }

            return new FeedbackDto
            {
                FeedbackId = feedback.Id,
                PatientId = feedback.PatientId,
                PatientName = patientUser?.FullName ?? string.Empty,
                DoctorId = feedback.DoctorId,
                DoctorName = doctorUser?.FullName ?? string.Empty,
                AppointmentId = feedback.AppointmentId,
                Rating = feedback.Rating,
                Comment = feedback.Comment ?? string.Empty,
                CreatedAt = feedback.CreatedAt
            };
        }
    }
}
