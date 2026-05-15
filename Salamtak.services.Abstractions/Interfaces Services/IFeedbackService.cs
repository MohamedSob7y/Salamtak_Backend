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
        Task<ApiResponse<FeedbackDto>> CreateAsync(Guid patientId, CreateFeedbackDto dto);

        Task<ApiResponse<FeedbackDto>> UpdateAsync(Guid patientId, UpdateFeedbackDto dto);

        Task<ApiResponse<IReadOnlyList<DoctorFeedbackDto>>> GetDoctorFeedbacksAsync(Guid doctorId);

        Task<ApiResponse> DeleteAsync(Guid feedbackId);
    }
}
