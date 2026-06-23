using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.Feedbacks;

namespace Salamtak.Web.Api.Controllers
{
    [Route("api/feedbacks")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost("patients/{patientId:guid}")]
        public async Task<IActionResult> CreateFeedback(Guid patientId, [FromBody] CreateFeedbackDto dto)
        {
            var response = await _feedbackService.CreateAsync(patientId, dto);
            return Ok(response);
        }

        [HttpPut("patients/{patientId:guid}")]
        public async Task<IActionResult> UpdateFeedback(Guid patientId, [FromBody] UpdateFeedbackDto dto)
        {
            var response = await _feedbackService.UpdateAsync(patientId, dto);
            return Ok(response);
        }

        [HttpGet("doctors/{doctorId:guid}")]
        public async Task<IActionResult> GetDoctorFeedbacks(Guid doctorId)
        {
            var response = await _feedbackService.GetDoctorFeedbacksAsync(doctorId);
            return Ok(response);
        }

        [HttpDelete("patients/{patientId:guid}/{feedbackId:guid}")]
public async Task<IActionResult> DeleteFeedback(Guid patientId, Guid feedbackId)
{
    var response = await _feedbackService.DeleteAsync(patientId, feedbackId);
    return Ok(response);
}
    }
}
