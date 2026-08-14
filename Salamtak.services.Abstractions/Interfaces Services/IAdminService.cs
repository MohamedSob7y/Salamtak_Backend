using Salamtak.Shared.DTOs.Admin;
using Salamtak.Shared.DTOs.Users;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IAdminService
    {
        Task<ApiResponse<IReadOnlyList<DoctorVerificationRequestDto>>>
          GetPendingDoctorsAsync();

        Task<ApiResponse> VerifyDoctorAsync(
            Guid adminUserId,
            DoctorVerificationResultDto dto);

        Task<ApiResponse> RejectDoctorAsync(
            Guid adminUserId,
            DoctorVerificationResultDto dto);

        Task<ApiResponse<IReadOnlyList<UserDto>>>
            GetUsersAsync();

        Task<ApiResponse> UpdateUserStatusAsync(
            Guid adminUserId,
            UpdateUserStatusDto dto);

        Task<ApiResponse<AdminDashboardStatsDto>>
            GetDashboardStatsAsync();
    }
}

