    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Salamtak.Domain.Models;
    using Salamtak.services.Abstractions.Interfaces_Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    namespace Salamtak.Presentation.Controllers
    {
        [Route("api/profile")]
        [ApiController]
        [Authorize(Roles = "Patient,Doctor")]
        public class ProfileController : ControllerBase
        {
            private readonly IProfileImageService _profileImageService;
            public ProfileController(IProfileImageService profileImageService)
            {
                _profileImageService = profileImageService;
            }
            [HttpPost("me/image")]
            [Consumes("multipart/form-data")]
            public async Task<IActionResult> UploadProfileImage(IFormFile image,CancellationToken cancellationToken)
            {
                var currentUserId = GetCurrentUserId();

                var response = await _profileImageService
                    .UploadProfileImageAsync(
                        currentUserId,
                        image,
                        cancellationToken);

                return Ok(response);
            }
            [HttpDelete("me/image")]
            public async Task<IActionResult> DeleteProfileImage(CancellationToken cancellationToken)
            {
                var currentUserId = GetCurrentUserId();

                var response = await _profileImageService
                    .DeleteProfileImageAsync(
                        currentUserId,
                        cancellationToken);

                return Ok(response);
            }
            private Guid GetCurrentUserId()
            {
                var userIdClaim =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub")
                    ?? User.FindFirstValue("userId")
                    ?? User.FindFirstValue("uid");

                if (string.IsNullOrWhiteSpace(userIdClaim) ||
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
