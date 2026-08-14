//using AutoMapper;
//using FluentValidation;
//using Salamtak.Domain.Interfaces.UnitOfWork;
//using Salamtak.Domain.Models;
//using Salamtak.Domain.Models.Enums;
//using Salamtak.services.Abstractions.Interfaces_Services;
//using Salamtak.services.Exceptions;
//using Salamtak.Shared.DTOs.Admin;
//using Salamtak.Shared.DTOs.Users;
//using Salamtak.Shared.Responses;

//namespace Salamtak.services.Implementation_Of_Services
//{
//    public class AdminService : IAdminService
//    {
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly IMapper _mapper;

//        private readonly IValidator<DoctorVerificationResultDto>_doctorVerificationValidator;

//        private readonly IValidator<UpdateUserStatusDto>_updateUserStatusValidator;

//        public AdminService(
//            IUnitOfWork unitOfWork,
//            IMapper mapper,
//            IValidator<DoctorVerificationResultDto>
//                doctorVerificationValidator,
//            IValidator<UpdateUserStatusDto>
//                updateUserStatusValidator)
//        {
//            _unitOfWork = unitOfWork;
//            _mapper = mapper;

//            _doctorVerificationValidator =
//                doctorVerificationValidator;

//            _updateUserStatusValidator =
//                updateUserStatusValidator;
//        }

//        public async Task<ApiResponse<IReadOnlyList<DoctorVerificationRequestDto>>>GetPendingDoctorsAsync()
//        {
//            var doctors = await _unitOfWork
//                .Repository<Doctor>()
//                .GetAllWithIncludesAsync(
//                    doctor =>
//                        doctor.VerificationStatus ==
//                        DoctorVerificationStatus.Pending,
//                    doctor => doctor.User,
//                    doctor => doctor.Specialty,
//                    doctor => doctor.DoctorDocuments);

//            var result = _mapper.Map<
//                IReadOnlyList<DoctorVerificationRequestDto>>(
//                doctors);

//            return ApiResponse<
//                IReadOnlyList<DoctorVerificationRequestDto>>.Ok(
//                result,
//                "Pending doctors retrieved successfully.");
//        }

//        public async Task<ApiResponse> VerifyDoctorAsync(Guid adminUserId,DoctorVerificationResultDto dto)
//        {
//            dto.IsApproved = true;

//            var validationResult =
//                await _doctorVerificationValidator
//                    .ValidateAsync(dto);

//            if (!validationResult.IsValid)
//            {
//                throw new AppValidationException(
//                    validationResult.Errors.Select(
//                        error => error.ErrorMessage));
//            }

//            var admin = await _unitOfWork
//                .Repository<Admin>()
//                .FirstOrDefaultAsync(admin =>
//                    admin.UserId == adminUserId);

//            if (admin is null)
//            {
//                throw new NotFoundException(
//                    "Admin profile not found.");
//            }

//            var doctor = await _unitOfWork
//                .Repository<Doctor>()
//                .GetByIdAsync(dto.DoctorId);

//            if (doctor is null)
//            {
//                throw new NotFoundException(
//                    "Doctor not found.");
//            }

//            if (doctor.VerificationStatus ==
//                    DoctorVerificationStatus.Verified &&
//                doctor.IsVerified)
//            {
//                throw new ConflictException(
//                    "Doctor is already verified.");
//            }

//            var documents = await _unitOfWork
//                .Repository<DoctorDocument>()
//                .GetAllAsync(document =>
//                    document.DoctorId == doctor.Id);

//            if (!documents.Any())
//            {
//                throw new BadRequestException(
//                    "Doctor has no uploaded documents.");
//            }

//            doctor.IsVerified = true;

//            doctor.VerificationStatus =
//                DoctorVerificationStatus.Verified;

//            foreach (var document in documents)
//            {
//                document.IsVerified = true;

//                document.VerifiedByAdminId =
//                    admin.Id;

//                document.VerifiedAt =
//                    DateTime.UtcNow;

//                document.RejectionReason = null;

//                _unitOfWork
//                    .Repository<DoctorDocument>()
//                    .Update(document);
//            }

//            _unitOfWork
//                .Repository<Doctor>()
//                .Update(doctor);

//            await _unitOfWork.SaveChangesAsync();

//            return ApiResponse.Ok(
//                "Doctor verified successfully.");
//        }

//        public async Task<ApiResponse> RejectDoctorAsync(Guid adminUserId,DoctorVerificationResultDto dto)
//        {
//            dto.IsApproved = false;

//            var validationResult =
//                await _doctorVerificationValidator
//                    .ValidateAsync(dto);

//            if (!validationResult.IsValid)
//            {
//                throw new AppValidationException(
//                    validationResult.Errors.Select(
//                        error => error.ErrorMessage));
//            }

//            var admin = await _unitOfWork
//                .Repository<Admin>()
//                .FirstOrDefaultAsync(admin =>
//                    admin.UserId == adminUserId);

//            if (admin is null)
//            {
//                throw new NotFoundException(
//                    "Admin profile not found.");
//            }

//            var doctor = await _unitOfWork
//                .Repository<Doctor>()
//                .GetByIdAsync(dto.DoctorId);

//            if (doctor is null)
//            {
//                throw new NotFoundException(
//                    "Doctor not found.");
//            }

//            if (doctor.VerificationStatus ==
//                DoctorVerificationStatus.Rejected)
//            {
//                throw new ConflictException(
//                    "Doctor is already rejected.");
//            }

//            if (string.IsNullOrWhiteSpace(
//                    dto.RejectionReason))
//            {
//                throw new BadRequestException(
//                    "Rejection reason is required.");
//            }

//            doctor.IsVerified = false;

//            doctor.VerificationStatus =
//                DoctorVerificationStatus.Rejected;

//            _unitOfWork
//                .Repository<Doctor>()
//                .Update(doctor);

//            var documents = await _unitOfWork
//                .Repository<DoctorDocument>()
//                .GetAllAsync(document =>
//                    document.DoctorId == doctor.Id);

//            foreach (var document in documents)
//            {
//                document.IsVerified = false;

//                document.VerifiedByAdminId =
//                    admin.Id;

//                document.VerifiedAt =
//                    DateTime.UtcNow;

//                document.RejectionReason =
//                    dto.RejectionReason.Trim();

//                _unitOfWork
//                    .Repository<DoctorDocument>()
//                    .Update(document);
//            }

//            await _unitOfWork.SaveChangesAsync();

//            return ApiResponse.Ok(
//                "Doctor rejected successfully.");
//        }

//        public async Task<ApiResponse<IReadOnlyList<UserDto>>>GetUsersAsync()
//        {
//            var users = await _unitOfWork
//                .Repository<User>()
//                .GetAllAsync();

//            var result =
//                _mapper.Map<IReadOnlyList<UserDto>>(
//                    users);

//            return ApiResponse<
//                IReadOnlyList<UserDto>>.Ok(
//                    result);
//        }

//        public async Task<ApiResponse>UpdateUserStatusAsync(Guid adminUserId,UpdateUserStatusDto dto)
//        {
//            var validationResult =
//                await _updateUserStatusValidator
//                    .ValidateAsync(dto);

//            if (!validationResult.IsValid)
//            {
//                throw new AppValidationException(
//                    validationResult.Errors.Select(
//                        error => error.ErrorMessage));
//            }

//            if (dto.UserId == adminUserId)
//            {
//                throw new ForbiddenException(
//                    "Admin cannot change their own account status.");
//            }

//            var user = await _unitOfWork
//                .Repository<User>()
//                .GetByIdAsync(dto.UserId);

//            if (user is null)
//            {
//                throw new NotFoundException(
//                    "User not found.");
//            }

//            if (user.Role == UserRole.Admin)
//            {
//                throw new ForbiddenException(
//                    "Admin accounts cannot be managed from this endpoint.");
//            }

//            if (!Enum.TryParse<UserStatus>(
//                    dto.Status,
//                    true,
//                    out var status))
//            {
//                throw new BadRequestException(
//                    "Invalid user status.");
//            }

//            if (user.Status == status)
//            {
//                throw new ConflictException(
//                    $"User is already {status}.");
//            }

//            user.Status = status;

//            _unitOfWork
//                .Repository<User>()
//                .Update(user);

//            await _unitOfWork.SaveChangesAsync();

//            return ApiResponse.Ok(
//                "User status updated successfully.");
//        }

//        public async Task<ApiResponse<AdminDashboardStatsDto>>GetDashboardStatsAsync()
//        {
//            var users = await _unitOfWork
//                .Repository<User>()
//                .GetAllAsync();

//            var doctors = await _unitOfWork
//                .Repository<Doctor>()
//                .GetAllAsync();

//            var admins = await _unitOfWork
//                .Repository<Admin>()
//                .GetAllAsync();

//            var patients = await _unitOfWork
//                .Repository<Patient>()
//                .GetAllAsync();

//            var appointments = await _unitOfWork
//                .Repository<Appointment>()
//                .GetAllAsync();

//            var stats =
//                new AdminDashboardStatsDto
//                {
//                    TotalUsers = users.Count,

//                    TotalPatients =
//                        patients.Count,

//                    TotalDoctors =
//                        doctors.Count,

//                    TotalAdmins =
//                        admins.Count,

//                    VerifiedDoctors =
//                        doctors.Count(doctor =>
//                            doctor.VerificationStatus ==
//                            DoctorVerificationStatus
//                                .Verified),

//                    PendingDoctors =
//                        doctors.Count(doctor =>
//                            doctor.VerificationStatus ==
//                            DoctorVerificationStatus
//                                .Pending),

//                    RejectedDoctors =
//                        doctors.Count(doctor =>
//                            doctor.VerificationStatus ==
//                            DoctorVerificationStatus
//                                .Rejected),

//                    TotalAppointments =
//                        appointments.Count,

//                    CompletedAppointments =
//                        appointments.Count(
//                            appointment =>
//                                appointment.Status ==
//                                AppointmentStatus
//                                    .Completed),

//                    CancelledAppointments =
//                        appointments.Count(
//                            appointment =>
//                                appointment.Status ==
//                                AppointmentStatus
//                                    .Cancelled)
//                };

//            return ApiResponse<
//                AdminDashboardStatsDto>.Ok(
//                    stats,
//                    "Dashboard statistics retrieved successfully.");
//        }
//    }
//}



using AutoMapper;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.Admin;
using Salamtak.Shared.DTOs.Notifications;
using Salamtak.Shared.DTOs.Users;
using Salamtak.Shared.Responses;

namespace Salamtak.services.Implementation_Of_Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        private readonly IValidator<DoctorVerificationResultDto>
            _doctorVerificationValidator;

        private readonly IValidator<UpdateUserStatusDto>
            _updateUserStatusValidator;

        public AdminService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            IValidator<DoctorVerificationResultDto>
                doctorVerificationValidator,
            IValidator<UpdateUserStatusDto>
                updateUserStatusValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;

            _doctorVerificationValidator =
                doctorVerificationValidator;

            _updateUserStatusValidator =
                updateUserStatusValidator;
        }

        public async Task<ApiResponse<IReadOnlyList<DoctorVerificationRequestDto>>>GetPendingDoctorsAsync()
        {
            var doctors = await _unitOfWork
                .Repository<Doctor>()
                .GetAllWithIncludesAsync(
                    doctor =>
                        doctor.VerificationStatus ==
                        DoctorVerificationStatus.Pending,
                    doctor => doctor.User,
                    doctor => doctor.Specialty,
                    doctor => doctor.DoctorDocuments);

            var result = _mapper.Map<
                IReadOnlyList<DoctorVerificationRequestDto>>(
                doctors);

            return ApiResponse<
                IReadOnlyList<DoctorVerificationRequestDto>>.Ok(
                result,
                "Pending doctors retrieved successfully.");
        }

        public async Task<ApiResponse> VerifyDoctorAsync(Guid adminUserId,DoctorVerificationResultDto dto)
        {
            dto.IsApproved = true;

            var validationResult =
                await _doctorVerificationValidator
                    .ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors.Select(
                        error => error.ErrorMessage));
            }

            var admin = await _unitOfWork
                .Repository<Admin>()
                .FirstOrDefaultAsync(admin =>
                    admin.UserId == adminUserId);

            if (admin is null)
            {
                throw new NotFoundException(
                    "Admin profile not found.");
            }

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(dto.DoctorId);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor not found.");
            }

            if (doctor.VerificationStatus ==
                    DoctorVerificationStatus.Verified &&
                doctor.IsVerified)
            {
                throw new ConflictException(
                    "Doctor is already verified.");
            }

            var documents = await _unitOfWork
                .Repository<DoctorDocument>()
                .GetAllAsync(document =>
                    document.DoctorId == doctor.Id);

            if (!documents.Any())
            {
                throw new BadRequestException(
                    "Doctor has no uploaded documents.");
            }

            doctor.IsVerified = true;

            doctor.VerificationStatus =
                DoctorVerificationStatus.Verified;

            foreach (var document in documents)
            {
                document.IsVerified = true;

                document.VerifiedByAdminId =
                    admin.Id;

                document.VerifiedAt =
                    DateTime.UtcNow;

                document.RejectionReason = null;

                _unitOfWork
                    .Repository<DoctorDocument>()
                    .Update(document);
            }

            _unitOfWork
                .Repository<Doctor>()
                .Update(doctor);

            await _unitOfWork.SaveChangesAsync();

            await _notificationService.CreateAsync(
                new CreateNotificationDto
                {
                    UserId = doctor.UserId,
                    AppointmentId = null,
                    Title = "Doctor Profile Approved",
                    Message =
                        "Your doctor profile has been approved successfully. You can now use the doctor features in Salamtak.",
                    Type =
                        NotificationType.General.ToString(),
                    Channel =
                        NotificationChannel
                            .InAppAndEmail
                            .ToString()
                });

            return ApiResponse.Ok(
                "Doctor verified successfully.");
        }

        public async Task<ApiResponse> RejectDoctorAsync(Guid adminUserId,DoctorVerificationResultDto dto)
        {
            dto.IsApproved = false;

            var validationResult =
                await _doctorVerificationValidator
                    .ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors.Select(
                        error => error.ErrorMessage));
            }

            var admin = await _unitOfWork
                .Repository<Admin>()
                .FirstOrDefaultAsync(admin =>
                    admin.UserId == adminUserId);

            if (admin is null)
            {
                throw new NotFoundException(
                    "Admin profile not found.");
            }

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(dto.DoctorId);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor not found.");
            }

            if (doctor.VerificationStatus ==
                DoctorVerificationStatus.Rejected)
            {
                throw new ConflictException(
                    "Doctor is already rejected.");
            }

            if (string.IsNullOrWhiteSpace(
                    dto.RejectionReason))
            {
                throw new BadRequestException(
                    "Rejection reason is required.");
            }

            var rejectionReason =
                dto.RejectionReason.Trim();

            doctor.IsVerified = false;

            doctor.VerificationStatus =
                DoctorVerificationStatus.Rejected;

            _unitOfWork
                .Repository<Doctor>()
                .Update(doctor);

            var documents = await _unitOfWork
                .Repository<DoctorDocument>()
                .GetAllAsync(document =>
                    document.DoctorId == doctor.Id);

            foreach (var document in documents)
            {
                document.IsVerified = false;

                document.VerifiedByAdminId =
                    admin.Id;

                document.VerifiedAt =
                    DateTime.UtcNow;

                document.RejectionReason =
                    rejectionReason;

                _unitOfWork
                    .Repository<DoctorDocument>()
                    .Update(document);
            }

            await _unitOfWork.SaveChangesAsync();

            await _notificationService.CreateAsync(
                new CreateNotificationDto
                {
                    UserId = doctor.UserId,
                    AppointmentId = null,
                    Title = "Doctor Profile Rejected",
                    Message =
                        $"Your doctor profile has been rejected. Reason: {rejectionReason}",
                    Type =
                        NotificationType.General.ToString(),
                    Channel =
                        NotificationChannel
                            .InAppAndEmail
                            .ToString()
                });

            return ApiResponse.Ok(
                "Doctor rejected successfully.");
        }

        public async Task<ApiResponse<IReadOnlyList<UserDto>>>GetUsersAsync()
        {
            var users = await _unitOfWork
                .Repository<User>()
                .GetAllAsync();

            var result =
                _mapper.Map<IReadOnlyList<UserDto>>(
                    users);

            return ApiResponse<
                IReadOnlyList<UserDto>>.Ok(
                    result);
        }

        public async Task<ApiResponse>UpdateUserStatusAsync(Guid adminUserId,UpdateUserStatusDto dto)
        {
            var validationResult =
                await _updateUserStatusValidator
                    .ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors.Select(
                        error => error.ErrorMessage));
            }

            if (dto.UserId == adminUserId)
            {
                throw new ForbiddenException(
                    "Admin cannot change their own account status.");
            }

            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(dto.UserId);

            if (user is null)
            {
                throw new NotFoundException(
                    "User not found.");
            }

            if (user.Role == UserRole.Admin)
            {
                throw new ForbiddenException(
                    "Admin accounts cannot be managed from this endpoint.");
            }

            if (!Enum.TryParse<UserStatus>(
                    dto.Status,
                    true,
                    out var status))
            {
                throw new BadRequestException(
                    "Invalid user status.");
            }

            if (user.Status == status)
            {
                throw new ConflictException(
                    $"User is already {status}.");
            }

            user.Status = status;

            _unitOfWork
                .Repository<User>()
                .Update(user);

            await _unitOfWork.SaveChangesAsync();

            var profileName = user.Role switch
            {
                UserRole.Patient => "patient",
                UserRole.Doctor => "doctor",
                _ => "user"
            };

            var notificationTitle = status switch
            {
                UserStatus.Active =>
                    "Profile Activated",

                UserStatus.Suspended =>
                    "Account Suspended",

                _ =>
                    "Account Status Updated"
            };

            var notificationMessage = status switch
            {
                UserStatus.Active =>
                    $"Your {profileName} profile has been activated successfully.",

                UserStatus.Suspended =>
                    $"Your {profileName} account has been suspended. Please contact Salamtak support for more information.",

                _ =>
                    $"Your account status has been changed to {status}."
            };

            await _notificationService.CreateAsync(
                new CreateNotificationDto
                {
                    UserId = user.Id,
                    AppointmentId = null,
                    Title = notificationTitle,
                    Message = notificationMessage,
                    Type =
                        NotificationType.General.ToString(),
                    Channel =
                        NotificationChannel
                            .InAppAndEmail
                            .ToString()
                });

            return ApiResponse.Ok(
                "User status updated successfully.");
        }

        public async Task<ApiResponse<AdminDashboardStatsDto>>GetDashboardStatsAsync()
        {
            var users = await _unitOfWork
                .Repository<User>()
                .GetAllAsync();

            var doctors = await _unitOfWork
                .Repository<Doctor>()
                .GetAllAsync();

            var admins = await _unitOfWork
                .Repository<Admin>()
                .GetAllAsync();

            var patients = await _unitOfWork
                .Repository<Patient>()
                .GetAllAsync();

            var appointments = await _unitOfWork
                .Repository<Appointment>()
                .GetAllAsync();

            var stats =
                new AdminDashboardStatsDto
                {
                    TotalUsers =
                        users.Count,

                    TotalPatients =
                        patients.Count,

                    TotalDoctors =
                        doctors.Count,

                    TotalAdmins =
                        admins.Count,

                    VerifiedDoctors =
                        doctors.Count(doctor =>
                            doctor.VerificationStatus ==
                            DoctorVerificationStatus
                                .Verified),

                    PendingDoctors =
                        doctors.Count(doctor =>
                            doctor.VerificationStatus ==
                            DoctorVerificationStatus
                                .Pending),

                    RejectedDoctors =
                        doctors.Count(doctor =>
                            doctor.VerificationStatus ==
                            DoctorVerificationStatus
                                .Rejected),

                    TotalAppointments =
                        appointments.Count,

                    CompletedAppointments =
                        appointments.Count(
                            appointment =>
                                appointment.Status ==
                                AppointmentStatus
                                    .Completed),

                    CancelledAppointments =
                        appointments.Count(
                            appointment =>
                                appointment.Status ==
                                AppointmentStatus
                                    .Cancelled)
                };

            return ApiResponse<
                AdminDashboardStatsDto>.Ok(
                    stats,
                    "Dashboard statistics retrieved successfully.");
        }
    }
}
