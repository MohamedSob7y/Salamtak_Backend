using Salamtak.Shared.DTOs.Clinics;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IClinicService
    {
        Task<ApiResponse<IReadOnlyList<ClinicDto>>> GetDoctorClinicsAsync(Guid doctorId);

        Task<ApiResponse<ClinicDto>> GetByIdAsync(Guid clinicId);

        Task<ApiResponse<ClinicDto>> CreateAsync(Guid doctorId, CreateClinicDto dto);

        Task<ApiResponse<ClinicDto>> UpdateAsync(Guid doctorId, Guid clinicId, UpdateClinicDto dto);

        Task<ApiResponse> DeleteAsync(Guid doctorId, Guid clinicId);
    }
}
