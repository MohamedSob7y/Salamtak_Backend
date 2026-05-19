using AutoMapper;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.Admin;
using Salamtak.Shared.DTOs.Users;
using Salamtak.Shared.Responses;

namespace Salamtak.services.Implementation_Of_Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<DoctorVerificationResultDto> _doctorVerificationValidator;
        private readonly IValidator<UpdateUserStatusDto> _updateUserStatusValidator;

        public AdminService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<DoctorVerificationResultDto> doctorVerificationValidator,
            IValidator<UpdateUserStatusDto> updateUserStatusValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _doctorVerificationValidator = doctorVerificationValidator;
            _updateUserStatusValidator = updateUserStatusValidator;
        }

        public async Task<ApiResponse<IReadOnlyList<DoctorVerificationRequestDto>>> GetPendingDoctorsAsync()
        {
            var doctors = await _unitOfWork
                .Repository<Doctor>()
                .GetAllAsync(d => d.VerificationStatus == DoctorVerificationStatus.Pending);

            var result = _mapper.Map<IReadOnlyList<DoctorVerificationRequestDto>>(doctors);

            return ApiResponse<IReadOnlyList<DoctorVerificationRequestDto>>.Ok(result);
        }

        public async Task<ApiResponse> VerifyDoctorAsync(Guid adminId, DoctorVerificationResultDto dto)
        {
            var validationResult = await _doctorVerificationValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var adminExists = await _unitOfWork
                .Repository<Admin>()
                .AnyAsync(a => a.Id == adminId);

            if (!adminExists)
                throw new NotFoundException("Admin not found.");

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(dto.DoctorId);

            if (doctor is null)
                throw new NotFoundException("Doctor not found.");

            if (doctor.VerificationStatus == DoctorVerificationStatus.Verified && doctor.IsVerified)
                throw new ConflictException("Doctor is already verified.");

            var documents = await _unitOfWork
                .Repository<DoctorDocument>()
                .GetAllAsync(d => d.DoctorId == doctor.Id);

            if (!documents.Any())
                throw new BadRequestException("Doctor has no uploaded documents.");

            doctor.IsVerified = true;
            doctor.VerificationStatus = DoctorVerificationStatus.Verified;

            foreach (var document in documents)
            {
                document.IsVerified = true;
                document.VerifiedByAdminId = adminId;
                document.VerifiedAt = DateTime.UtcNow;
                document.RejectionReason = null;

                _unitOfWork.Repository<DoctorDocument>().Update(document);
            }

            _unitOfWork.Repository<Doctor>().Update(doctor);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Doctor verified successfully.");
        }

        public async Task<ApiResponse> RejectDoctorAsync(Guid adminId, DoctorVerificationResultDto dto)
        {
            var validationResult = await _doctorVerificationValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var adminExists = await _unitOfWork
                .Repository<Admin>()
                .AnyAsync(a => a.Id == adminId);

            if (!adminExists)
                throw new NotFoundException("Admin not found.");

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(dto.DoctorId);

            if (doctor is null)
                throw new NotFoundException("Doctor not found.");

            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                throw new BadRequestException("Rejection reason is required.");

            doctor.IsVerified = false;
            doctor.VerificationStatus = DoctorVerificationStatus.Rejected;

            _unitOfWork.Repository<Doctor>().Update(doctor);

            var documents = await _unitOfWork
                .Repository<DoctorDocument>()
                .GetAllAsync(d => d.DoctorId == doctor.Id);

            foreach (var document in documents)
            {
                document.IsVerified = false;
                document.VerifiedByAdminId = adminId;
                document.VerifiedAt = DateTime.UtcNow;
                document.RejectionReason = dto.RejectionReason.Trim();

                _unitOfWork.Repository<DoctorDocument>().Update(document);
            }

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Doctor rejected successfully.");
        }

        public async Task<ApiResponse<IReadOnlyList<UserDto>>> GetUsersAsync()
        {
            var users = await _unitOfWork
                .Repository<User>()
                .GetAllAsync();

            var result = _mapper.Map<IReadOnlyList<UserDto>>(users);

            return ApiResponse<IReadOnlyList<UserDto>>.Ok(result);
        }

        public async Task<ApiResponse> UpdateUserStatusAsync(UpdateUserStatusDto dto)
        {
            var validationResult = await _updateUserStatusValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(dto.UserId);

            if (user is null)
                throw new NotFoundException("User not found.");

            if (!Enum.TryParse<UserStatus>(dto.Status, true, out var status))
                throw new BadRequestException("Invalid user status.");

            user.Status = status;

            _unitOfWork.Repository<User>().Update(user);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("User status updated successfully.");
        }

        public async Task<ApiResponse<AdminDashboardStatsDto>> GetDashboardStatsAsync()
        {
            var doctors = await _unitOfWork
                .Repository<Doctor>()
                .GetAllAsync();

            var appointments = await _unitOfWork
                .Repository<Appointment>()
                .GetAllAsync();

            var patients = await _unitOfWork
                .Repository<Patient>()
                .GetAllAsync();

            var stats = new AdminDashboardStatsDto
            {
                TotalPatients = patients.Count,
                TotalDoctors = doctors.Count,
                VerifiedDoctors = doctors.Count(d => d.IsVerified),
                PendingDoctors = doctors.Count(d => d.VerificationStatus == DoctorVerificationStatus.Pending),
                TotalAppointments = appointments.Count,
                CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled)
            };

            return ApiResponse<AdminDashboardStatsDto>.Ok(stats);
        }
    }
}
