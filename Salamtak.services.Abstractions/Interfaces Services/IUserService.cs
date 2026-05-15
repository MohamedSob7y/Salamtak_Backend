using Salamtak.Shared.DTOs.Users;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IUserService
    {
        Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId);

        Task<ApiResponse<UserDto>> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto dto);

        Task<ApiResponse> SuspendUserAsync(Guid userId);

        Task<ApiResponse> ActivateUserAsync(Guid userId);

        Task<ApiResponse> SoftDeleteUserAsync(Guid userId);
    }
}
