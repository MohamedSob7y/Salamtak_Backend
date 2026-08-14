using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.Profile;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Implementation_Of_Services
{
    public class ProfileImageService : IProfileImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly IConfiguration _configuration;

        private const long MaxImageSizeInBytes =
            5 * 1024 * 1024; // 5 MB

        private static readonly string[] AllowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        private static readonly string[] AllowedContentTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        public ProfileImageService(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _configuration = configuration;
        }
        public async Task<ApiResponse<ProfileImageDto>>UploadProfileImageAsync(Guid userId,IFormFile image, CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(userId);

            if (user is null)
            {
                throw new NotFoundException(
                    "User not found.");
            }

            ValidateAllowedRole(user);
            ValidateImage(image);

            var folderName = GetProfileFolder(user);

            var oldImagePath = user.ProfileImagePath;

            var storedFile = await _fileStorageService
                .UploadAsync(
                    image,
                    folderName,
                    cancellationToken);

            try
            {
                user.ProfileImagePath =
                    storedFile.RelativePath;

                _unitOfWork
                    .Repository<User>()
                    .Update(user);

                await _unitOfWork.SaveChangesAsync();
            }
            catch
            {
                
                await _fileStorageService.DeleteAsync(
                    storedFile.RelativePath,
                    cancellationToken);

                throw;
            }

           
            if (!string.IsNullOrWhiteSpace(oldImagePath) &&
                !string.Equals(
                    oldImagePath,
                    storedFile.RelativePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                await _fileStorageService.DeleteAsync(
                    oldImagePath,
                    cancellationToken);
            }

            var result = new ProfileImageDto
            {
                UserId = user.Id,

                ProfileImagePath =
                    storedFile.RelativePath,

                ProfileImageUrl =
                    BuildPublicFileUrl(
                        storedFile.RelativePath)
            };

            return ApiResponse<ProfileImageDto>.Ok(
                result,
                "Profile image uploaded successfully.");
        }
        public async Task<ApiResponse<bool>>DeleteProfileImageAsync(Guid userId,CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(userId);

            if (user is null)
            {
                throw new NotFoundException(
                    "User not found.");
            }

            ValidateAllowedRole(user);

            if (string.IsNullOrWhiteSpace(
                user.ProfileImagePath))
            {
                throw new NotFoundException(
                    "Profile image not found.");
            }

            var oldImagePath =
                user.ProfileImagePath;

            user.ProfileImagePath = null;

            _unitOfWork
                .Repository<User>()
                .Update(user);

            await _unitOfWork.SaveChangesAsync();

            await _fileStorageService.DeleteAsync(
                oldImagePath,
                cancellationToken);

            return ApiResponse<bool>.Ok(
                true,
                "Profile image deleted successfully.");
        }
        private static void ValidateImage(IFormFile image)
        {
            if (image is null ||
                image.Length == 0)
            {
                throw new BadRequestException(
                    "Profile image is required.");
            }

            if (image.Length >
                MaxImageSizeInBytes)
            {
                throw new BadRequestException(
                    "Profile image size must not exceed 5 MB.");
            }

            var extension =
                Path.GetExtension(image.FileName)
                    .ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(extension) ||
                !AllowedExtensions.Contains(extension))
            {
                throw new BadRequestException(
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");
            }

            var contentType =
                image.ContentType
                    .ToLowerInvariant();

            if (!AllowedContentTypes.Contains(
                contentType))
            {
                throw new BadRequestException(
                    "Invalid image content type.");
            }
        }
        private static void ValidateAllowedRole(User user)
        {
            if (user.Role != UserRole.Patient &&
                user.Role != UserRole.Doctor)
            {
                throw new ForbiddenException(
                    "Only patients and doctors can upload profile images.");
            }
        }
        private static string GetProfileFolder(User user)
        {
            return user.Role switch
            {
                UserRole.Patient =>
                    "uploads/profile-images/patients",

                UserRole.Doctor =>
                    "uploads/profile-images/doctors",

                _ => throw new ForbiddenException(
                    "Unsupported user role.")
            };
        }
        private string BuildPublicFileUrl(string relativePath)
        {
            var baseUrl =
                _configuration["URLs:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException(
                    "URLs:BaseUrl is not configured.");
            }

            return
                $"{baseUrl.TrimEnd('/')}/" +
                $"{relativePath.TrimStart('/')}";
        }
    }
}
