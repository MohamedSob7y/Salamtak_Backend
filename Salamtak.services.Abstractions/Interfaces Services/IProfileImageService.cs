using Microsoft.AspNetCore.Http;
using Salamtak.Shared.DTOs.Profile;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IProfileImageService
    {
        Task<ApiResponse<ProfileImageDto>>UploadProfileImageAsync(Guid userId,IFormFile image,CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>>DeleteProfileImageAsync(Guid userId,CancellationToken cancellationToken = default);
    }
}
