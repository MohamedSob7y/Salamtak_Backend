using Salamtak.Shared.DTOs.Appointments;
using Salamtak.Shared.DTOs.Doctors;
using Salamtak.Shared.Responses;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IDoctorService
    {
        Task<ApiResponse<PagedResult<DoctorCardDto>>>
            SearchDoctorsAsync(
                DoctorSearchRequestDto dto);

        Task<ApiResponse<DoctorDetailsDto>>
            GetDoctorDetailsAsync(
                Guid doctorId);

        Task<ApiResponse<DoctorProfileDto>>
            GetProfileAsync(
                Guid doctorUserId);

        Task<ApiResponse<DoctorProfileDto>>
            UpdateProfileAsync(
                Guid doctorUserId,
                UpdateDoctorProfileDto dto);

        Task<ApiResponse<IReadOnlyList<DoctorAppointmentDto>>>
            GetAppointmentsAsync(
                Guid doctorUserId);

        Task<ApiResponse<DoctorRatingSummaryDto>>
            GetRatingSummaryAsync(
                Guid doctorUserId);
    }
}