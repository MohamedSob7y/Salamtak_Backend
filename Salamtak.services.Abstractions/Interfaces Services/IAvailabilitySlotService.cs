using Salamtak.Shared.DTOs.AvailabilitySlots;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IAvailabilitySlotService
    {
        Task<ApiResponse<IReadOnlyList<AvailabilitySlotDto>>> GetDoctorAvailableSlotsAsync(Guid doctorId);

        Task<ApiResponse<IReadOnlyList<AvailabilitySlotDto>>> GetClinicAvailableSlotsAsync(Guid clinicId);

        Task<ApiResponse<AvailabilitySlotDto>> GetByIdAsync(Guid slotId);

        Task<ApiResponse<IReadOnlyList<AvailabilitySlotDto>>> SearchAvailableSlotsAsync(AvailableSlotSearchDto dto);

        Task<ApiResponse<AvailabilitySlotDto>> CreateAsync(Guid doctorId, CreateAvailabilitySlotDto dto);

        Task<ApiResponse<AvailabilitySlotDto>> UpdateAsync(Guid doctorId, Guid slotId, UpdateAvailabilitySlotDto dto);

        Task<ApiResponse> DeleteAsync(Guid doctorId, Guid slotId);

        Task<ApiResponse> MarkAsAvailableAsync(Guid doctorId, Guid slotId);

        Task<ApiResponse> MarkAsUnavailableAsync(Guid doctorId, Guid slotId);
    }
}
