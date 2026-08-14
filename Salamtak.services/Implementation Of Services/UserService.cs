using AutoMapper;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.Users;
using Salamtak.Shared.Responses;

namespace Salamtak.services.Implementation_Of_Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<UpdateUserProfileDto> _updateValidator;

        public UserService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<UpdateUserProfileDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _updateValidator = updateValidator;
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId)
        {
            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(userId);

            if (user is null)
                throw new NotFoundException("User not found.");

            var result = _mapper.Map<UserDto>(user);

            return ApiResponse<UserDto>.Ok(result);
        }

        public async Task<ApiResponse<UserDto>> UpdateUserProfileAsync(
            Guid userId,
            UpdateUserProfileDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(userId);

            if (user is null)
                throw new NotFoundException("User not found.");

            var phoneNumber = dto.PhoneNumber.Trim();

            var phoneExists = await _unitOfWork
                .Repository<User>()
                .AnyAsync(u =>
                    u.Id != userId &&
                    u.PhoneNumber == phoneNumber &&
                    !u.IsDeleted);

            if (phoneExists)
                throw new ConflictException("Phone number already exists.");

            user.FullName = dto.FullName.Trim();
            user.PhoneNumber = phoneNumber;

            _unitOfWork
                .Repository<User>()
                .Update(user);

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<UserDto>(user);

            return ApiResponse<UserDto>.Ok(
                result,
                "User profile updated successfully.");
        }

        public async Task<ApiResponse> SuspendUserAsync(Guid userId)
        {
            return await ChangeStatusAsync(
                userId,
                UserStatus.Suspended,
                "User suspended successfully.");
        }

        public async Task<ApiResponse> ActivateUserAsync(Guid userId)
        {
            return await ChangeStatusAsync(
                userId,
                UserStatus.Active,
                "User activated successfully.");
        }

        public async Task<ApiResponse> SoftDeleteUserAsync(Guid userId)
        {
            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(userId);

            if (user is null)
                throw new NotFoundException("User not found.");

            if (user.Status == UserStatus.Suspended)
                throw new ConflictException("User is already suspended.");

            user.Status = UserStatus.Suspended;

            _unitOfWork
                .Repository<User>()
                .Update(user);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("User suspended successfully.");
        }

        private async Task<ApiResponse> ChangeStatusAsync(
            Guid userId,
            UserStatus status,
            string message)
        {
            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(userId);

            if (user is null)
                throw new NotFoundException("User not found.");

            if (user.Status == status)
                throw new ConflictException($"User is already {status}.");

            user.Status = status;

            _unitOfWork
                .Repository<User>()
                .Update(user);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok(message);
        }
    }
}
