using Salamtak.Shared.DTOs.AI;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IAIService
    {
        Task<ApiResponse<AiSpecialtyRecommendationDto>> RecommendSpecialtyAsync(AiSymptomRequestDto dto);

        Task<ApiResponse<AiDoctorRecommendationDto>> RecommendDoctorsAsync(AiSymptomRequestDto dto);
    }
}
