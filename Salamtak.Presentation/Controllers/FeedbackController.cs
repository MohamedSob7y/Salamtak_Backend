using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.Feedbacks;
using System.Security.Claims;

namespace Salamtak.Presentation.Controllers
{
    [Route("api/feedbacks")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IUnitOfWork _unitOfWork;

        public FeedbackController(
            IFeedbackService feedbackService,
            IUnitOfWork unitOfWork)
        {
            _feedbackService = feedbackService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackDto dto)
        {
            var patient = await GetCurrentPatientAsync();

            if (patient is null)
                return Forbid();

            var response = await _feedbackService.CreateAsync(patient.Id, dto);
            return Ok(response);
        }

        [HttpPut("{feedbackId:guid}")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> UpdateFeedback(Guid feedbackId, [FromBody] UpdateFeedbackDto dto)
        {
            dto.FeedbackId = feedbackId;

            var patient = await GetCurrentPatientAsync();

            if (patient is null)
                return Forbid();

            var response = await _feedbackService.UpdateAsync(patient.Id, dto);
            return Ok(response);
        }

        [HttpGet("doctors/{doctorId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoctorFeedbacks(Guid doctorId)
        {
            var response = await _feedbackService.GetDoctorFeedbacksAsync(doctorId);
            return Ok(response);
        }

        [HttpDelete("{feedbackId:guid}")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> DeleteFeedback(Guid feedbackId)
        {
            var patient = await GetCurrentPatientAsync();

            if (patient is null)
                return Forbid();

            var response = await _feedbackService.DeleteAsync(patient.Id, feedbackId);
            return Ok(response);
        }

        private Guid GetCurrentUserId()
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("sub") ??
                User.FindFirstValue("userId") ??
                User.FindFirstValue("uid");

            if (!Guid.TryParse(userId, out var parsedUserId))
                throw new UnauthorizedAccessException("Invalid or missing user id in token.");

            return parsedUserId;
        }

        private async Task<Patient?> GetCurrentPatientAsync()
        {
            var userId = GetCurrentUserId();

            return await _unitOfWork.Repository<Patient>()
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }
    }
}
