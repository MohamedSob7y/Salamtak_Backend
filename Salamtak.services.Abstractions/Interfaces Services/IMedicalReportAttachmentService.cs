using Salamtak.Shared.DTOs.Files;
using Salamtak.Shared.DTOs.MedicalReports;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{

    public interface IMedicalReportAttachmentService
    {
       
        Task<ApiResponse<MedicalReportAttachmentDto>>
            UploadPatientAttachmentAsync(
                Guid patientUserId,
                UploadPatientMedicalAttachmentDto dto,
                CancellationToken cancellationToken = default);

       
        Task<ApiResponse<MedicalReportAttachmentDto>>
            UploadDoctorAttachmentAsync(
                Guid doctorUserId,
                Guid medicalReportEntryId,
                UploadDoctorMedicalAttachmentDto dto,
                CancellationToken cancellationToken = default);

       
        Task<ApiResponse<IReadOnlyList<MedicalReportAttachmentDto>>>
            GetMyAttachmentsAsync(
                Guid patientUserId,
                CancellationToken cancellationToken = default);

       
        Task<ApiResponse<IReadOnlyList<MedicalReportAttachmentDto>>>
            GetPatientAttachmentsForDoctorAsync(
                Guid doctorUserId,
                Guid appointmentId,
                CancellationToken cancellationToken = default);

       
        Task<PrivateFileDownloadResult>
            DownloadPatientAttachmentAsync(
                Guid patientUserId,
                Guid attachmentId,
                CancellationToken cancellationToken = default);

       
        Task<PrivateFileDownloadResult>
            DownloadDoctorAttachmentAsync(
                Guid doctorUserId,
                Guid appointmentId,
                Guid attachmentId,
                CancellationToken cancellationToken = default);

       
        Task<ApiResponse<bool>>
            DeletePatientAttachmentAsync(
                Guid patientUserId,
                Guid attachmentId,
                CancellationToken cancellationToken = default);

        
        Task<ApiResponse<bool>>
            DeleteDoctorAttachmentAsync(
                Guid doctorUserId,
                Guid attachmentId,
                CancellationToken cancellationToken = default);
    }
}
