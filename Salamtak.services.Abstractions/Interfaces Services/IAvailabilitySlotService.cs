using Salamtak.Shared.DTOs.AvailabilitySlots;
using Salamtak.Shared.Responses;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IAvailabilitySlotService
    {
        Task<ApiResponse<IReadOnlyList<AvailabilitySlotDto>>>
            GetDoctorAvailableSlotsAsync(
                Guid doctorId);

        Task<ApiResponse<IReadOnlyList<AvailabilitySlotDto>>>
            GetClinicAvailableSlotsAsync(
                Guid clinicId);

        Task<ApiResponse<AvailabilitySlotDto>>
            GetByIdAsync(
                Guid slotId);

        Task<ApiResponse<IReadOnlyList<AvailabilitySlotDto>>>
            SearchAvailableSlotsAsync(
                AvailableSlotSearchDto dto);

        Task<ApiResponse<AvailabilitySlotDto>>
            CreateAsync(
                Guid doctorUserId,
                CreateAvailabilitySlotDto dto);

        Task<ApiResponse<AvailabilitySlotDto>>
            UpdateAsync(
                Guid doctorUserId,
                Guid slotId,
                UpdateAvailabilitySlotDto dto);

        Task<ApiResponse>
            DeleteAsync(
                Guid doctorUserId,
                Guid slotId);

        Task<ApiResponse>
            MarkAsAvailableAsync(
                Guid doctorUserId,
                Guid slotId);

        Task<ApiResponse>
            MarkAsUnavailableAsync(
                Guid doctorUserId,
                Guid slotId);
    }
}