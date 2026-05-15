using Salamtak.Shared.DTOs.Auth;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto);

        Task<ApiResponse<LoginResponseDto>> RegisterPatientAsync(RegisterPatientRequestDto dto);

        Task<ApiResponse<LoginResponseDto>> RegisterDoctorAsync(RegisterDoctorRequestDto dto);

        Task<ApiResponse<CurrentUserDto>> GetCurrentUserAsync(Guid userId);
    }
}
