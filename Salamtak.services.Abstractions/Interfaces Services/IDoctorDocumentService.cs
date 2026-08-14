using Salamtak.Shared.DTOs.DoctorDocuments;
using Salamtak.Shared.DTOs.Files;
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
        

        Task<ApiResponse<DoctorDocumentDto>>
            UploadMyDocumentAsync(
                Guid doctorUserId,
                UploadDoctorDocumentDto dto,
                CancellationToken cancellationToken = default);

        Task<ApiResponse<IReadOnlyList<DoctorDocumentDto>>>
            GetMyDocumentsAsync(
                Guid doctorUserId,
                CancellationToken cancellationToken = default);

        Task<PrivateFileDownloadResult>
            DownloadMyDocumentAsync(
                Guid doctorUserId,
                Guid documentId,
                CancellationToken cancellationToken = default);

        Task<ApiResponse<bool>>
            DeleteMyDocumentAsync(
                Guid doctorUserId,
                Guid documentId,
                CancellationToken cancellationToken = default);

        

        Task<ApiResponse<IReadOnlyList<DoctorDocumentDto>>>
            GetDoctorDocumentsForAdminAsync(
                Guid adminUserId,
                Guid doctorId,
                CancellationToken cancellationToken = default);

        Task<PrivateFileDownloadResult>
            DownloadDocumentForAdminAsync(
                Guid adminUserId,
                Guid documentId,
                CancellationToken cancellationToken = default);

        Task<ApiResponse<DoctorDocumentDto>>
            ApproveDocumentAsync(
                Guid adminUserId,
                Guid documentId,
                CancellationToken cancellationToken = default);

        Task<ApiResponse<DoctorDocumentDto>>
            RejectDocumentAsync(
                Guid adminUserId,
                Guid documentId,
                RejectDoctorDocumentDto dto,
                CancellationToken cancellationToken = default);
    }
}
