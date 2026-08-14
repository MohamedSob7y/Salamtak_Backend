using Salamtak.Shared.DTOs.Feedbacks;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IFeedbackService
    {
        Task<ApiResponse<FeedbackDto>> CreateAsync(Guid patientUserId,CreateFeedbackDto dto,CancellationToken cancellationToken = default);

        Task<ApiResponse<FeedbackDto>> UpdateAsync(Guid patientUserId,UpdateFeedbackDto dto,CancellationToken cancellationToken = default);

        Task<ApiResponse<IReadOnlyList<DoctorFeedbackDto>>>GetDoctorFeedbacksAsync(Guid doctorId,CancellationToken cancellationToken = default);

        Task<ApiResponse> DeleteAsync(Guid patientUserId,Guid feedbackId,CancellationToken cancellationToken = default);
    }
}
