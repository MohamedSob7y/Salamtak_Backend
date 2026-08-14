using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Salamtak.Domain.Models;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.Feedbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Presentation.Controllers
{
    [Route("api/feedbacks")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private const string FeedbackCacheTag =
            "doctor-feedbacks";

        private readonly IFeedbackService
            _feedbackService;

        private readonly IOutputCacheStore
            _outputCacheStore;

        public FeedbackController(
            IFeedbackService feedbackService,
            IOutputCacheStore outputCacheStore)
        {
            _feedbackService =
                feedbackService;

            _outputCacheStore =
                outputCacheStore;
        }

        [HttpPost]
        [Authorize(Roles = "Patient")]
        [EnableRateLimiting("FeedbackWrite")]
        public async Task<IActionResult>
            CreateFeedback(
                [FromBody] CreateFeedbackDto dto,
                CancellationToken cancellationToken)
        {
            var patientUserId =
                GetCurrentUserId();

            var response =
                await _feedbackService.CreateAsync(
                    patientUserId,
                    dto,
                    cancellationToken);

            await EvictFeedbackCacheAsync(
                cancellationToken);

            return Ok(response);
        }

        [HttpPut("{feedbackId:guid}")]
        [Authorize(Roles = "Patient")]
        [EnableRateLimiting("FeedbackWrite")]
        public async Task<IActionResult>
            UpdateFeedback(
                Guid feedbackId,
                [FromBody] UpdateFeedbackDto dto,
                CancellationToken cancellationToken)
        {
            dto.FeedbackId =
                feedbackId;

            var patientUserId =
                GetCurrentUserId();

            var response =
                await _feedbackService.UpdateAsync(
                    patientUserId,
                    dto,
                    cancellationToken);

            await EvictFeedbackCacheAsync(
                cancellationToken);

            return Ok(response);
        }

        [HttpGet("doctors/{doctorId:guid}")]
        [AllowAnonymous]
        [OutputCache(PolicyName = "PublicShort")]
        [EnableRateLimiting("FeedbackRead")]
        public async Task<IActionResult>
            GetDoctorFeedbacks(
                Guid doctorId,
                CancellationToken cancellationToken)
        {
            var response =
                await _feedbackService
                    .GetDoctorFeedbacksAsync(
                        doctorId,
                        cancellationToken);

            return Ok(response);
        }

        [HttpDelete("{feedbackId:guid}")]
        [Authorize(Roles = "Patient")]
        [EnableRateLimiting("FeedbackWrite")]
        public async Task<IActionResult>
            DeleteFeedback(
                Guid feedbackId,
                CancellationToken cancellationToken)
        {
            var patientUserId =
                GetCurrentUserId();

            var response =
                await _feedbackService.DeleteAsync(
                    patientUserId,
                    feedbackId,
                    cancellationToken);

            await EvictFeedbackCacheAsync(
                cancellationToken);

            return Ok(response);
        }

        private async Task EvictFeedbackCacheAsync(
            CancellationToken cancellationToken)
        {
            await _outputCacheStore.EvictByTagAsync(
                FeedbackCacheTag,
                cancellationToken);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("userId")
                ?? User.FindFirstValue("uid");

            if (string.IsNullOrWhiteSpace(
                    userIdClaim) ||
                !Guid.TryParse(
                    userIdClaim,
                    out var currentUserId))
            {
                throw new UnauthorizedAccessException(
                    "Invalid or missing user id in token.");
            }

            return currentUserId;
        }
    }
}
