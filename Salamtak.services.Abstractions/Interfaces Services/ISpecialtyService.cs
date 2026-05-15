using Salamtak.Shared.DTOs.Specialties;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface ISpecialtyService
    {
        Task<ApiResponse<IReadOnlyList<SpecialtyDto>>> GetAllAsync();

        Task<ApiResponse<SpecialtyDto>> GetByIdAsync(Guid specialtyId);

        Task<ApiResponse<SpecialtyDto>> CreateAsync(CreateSpecialtyDto dto);

        Task<ApiResponse<SpecialtyDto>> UpdateAsync(UpdateSpecialtyDto dto);

        Task<ApiResponse> DeleteAsync(Guid specialtyId);
    }
}
