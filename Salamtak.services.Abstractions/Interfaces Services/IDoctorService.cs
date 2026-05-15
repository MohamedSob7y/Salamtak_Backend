using Salamtak.Shared.DTOs.Appointments;
using Salamtak.Shared.DTOs.Doctors;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IDoctorService
    {
        Task<ApiResponse<PagedResult<DoctorCardDto>>> SearchDoctorsAsync(DoctorSearchRequestDto dto);

        Task<ApiResponse<DoctorDetailsDto>> GetDoctorDetailsAsync(Guid doctorId);

        Task<ApiResponse<DoctorProfileDto>> GetProfileAsync(Guid doctorId);

        Task<ApiResponse<DoctorProfileDto>> UpdateProfileAsync(Guid doctorId, UpdateDoctorProfileDto dto);

        Task<ApiResponse<IReadOnlyList<DoctorAppointmentDto>>> GetAppointmentsAsync(Guid doctorId);

        Task<ApiResponse<DoctorRatingSummaryDto>> GetRatingSummaryAsync(Guid doctorId);
    }
}
