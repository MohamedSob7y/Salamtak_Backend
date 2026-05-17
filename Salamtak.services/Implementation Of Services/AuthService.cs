using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.Auth;
using Salamtak.Shared.Responses;
using System.Security.Cryptography;
using System.Text;

namespace Salamtak.services.Implementation_Of_Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<LoginRequestDto> _loginValidator;
        private readonly IValidator<RegisterPatientRequestDto> _patientValidator;
        private readonly IValidator<RegisterDoctorRequestDto> _doctorValidator;

        public AuthService(
            IUnitOfWork unitOfWork,
            IValidator<LoginRequestDto> loginValidator,
            IValidator<RegisterPatientRequestDto> patientValidator,
            IValidator<RegisterDoctorRequestDto> doctorValidator)
        {
            _unitOfWork = unitOfWork;
            _loginValidator = loginValidator;
            _patientValidator = patientValidator;
            _doctorValidator = doctorValidator;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            var validationResult = await _loginValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var user = await _unitOfWork.Repository<User>().FirstOrDefaultAsync(u => u.Email == dto.Email.Trim());

            if (user is null || !VerifyPassword(dto.Password, user.PasswordHash))
                throw new BadRequestException("Invalid email or password.");

            return ApiResponse<LoginResponseDto>.Ok(ToLoginResponse(user), "Login successful.");
        }

        public async Task<ApiResponse<LoginResponseDto>> RegisterPatientAsync(RegisterPatientRequestDto dto)
        {
            var validationResult = await _patientValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var emailExists = await _unitOfWork.Repository<User>().AnyAsync(u => u.Email == dto.Email.Trim());
            if (emailExists)
                throw new ConflictException("Email already exists.");

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                PasswordHash = HashPassword(dto.Password),
                Role = UserRole.Patient
            };

            var patient = new Patient
            {
                User = user,
                DateOfBirth = dto.DateOfBirth,
                Gender = Enum.Parse<Gender>(dto.Gender, true),
                Address = dto.Address?.Trim(),
                BloodType = dto.BloodType?.Trim(),
                Height = dto.Height,
                Weight = dto.Weight
            };

            var medicalReport = new MedicalReport { Patient = patient };

            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.Repository<Patient>().AddAsync(patient);
            await _unitOfWork.Repository<MedicalReport>().AddAsync(medicalReport);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<LoginResponseDto>.Ok(ToLoginResponse(user), "Patient registered successfully.");
        }

        public async Task<ApiResponse<LoginResponseDto>> RegisterDoctorAsync(RegisterDoctorRequestDto dto)
        {
            var validationResult = await _doctorValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var emailExists = await _unitOfWork.Repository<User>().AnyAsync(u => u.Email == dto.Email.Trim());
            if (emailExists)
                throw new ConflictException("Email already exists.");

            var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(dto.SpecialtyId);
            if (specialty is null)
                throw new NotFoundException("Specialty not found.");

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                PasswordHash = HashPassword(dto.Password),
                Role = UserRole.Doctor
            };

            var doctor = new Doctor
            {
                User = user,
                SpecialtyId = dto.SpecialtyId,
                LicenseNumber = dto.LicenseNumber.Trim(),
                Bio = dto.Bio?.Trim(),
                ConsultationFee = dto.ConsultationFee,
                ExperienceYears = dto.ExperienceYears,
                VerificationStatus = DoctorVerificationStatus.Pending,
                IsVerified = false
            };

            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.Repository<Doctor>().AddAsync(doctor);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<LoginResponseDto>.Ok(ToLoginResponse(user), "Doctor registered successfully.");
        }

        public async Task<ApiResponse<CurrentUserDto>> GetCurrentUserAsync(Guid userId)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user is null)
                throw new NotFoundException("User not found.");

            var result = new CurrentUserDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            };

            return ApiResponse<CurrentUserDto>.Ok(result);
        }

        private static LoginResponseDto ToLoginResponse(User user)
        {
            return new LoginResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                Expiration = DateTime.UtcNow.AddDays(1)
            };
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            var hash = HashPassword(password);
            return string.Equals(hash, storedHash, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(password, storedHash, StringComparison.Ordinal);
        }
    }
}
