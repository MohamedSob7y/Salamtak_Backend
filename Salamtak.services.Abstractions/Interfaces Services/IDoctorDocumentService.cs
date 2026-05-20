using Salamtak.Shared.DTOs.DoctorDocuments;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IDoctorDocumentService
    {
        Task<ApiResponse<DoctorDocumentDto>> UploadAsync(Guid doctorId, UploadDoctorDocumentDto dto);

        Task<ApiResponse<IReadOnlyList<DoctorDocumentDto>>> GetDoctorDocumentsAsync(Guid doctorId);

        Task<ApiResponse> VerifyAsync(VerifyDoctorDocumentDto dto);

        Task<ApiResponse> RejectAsync(RejectDoctorDocumentDto dto);
    }
}
