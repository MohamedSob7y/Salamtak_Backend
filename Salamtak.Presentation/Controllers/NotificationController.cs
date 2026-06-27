using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.Notifications;
using System.Security.Claims;

namespace Salamtak.Presentation.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            var response = await _notificationService.CreateAsync(dto);
            return Ok(response);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Patient,Doctor,Admin")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = GetCurrentUserId();

            var response = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(response);
        }

        [HttpGet("users/{userId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserNotifications(Guid userId)
        {
            var response = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(response);
        }

        [HttpPut("{notificationId:guid}/read")]
        [Authorize(Roles = "Patient,Doctor,Admin")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            var userId = GetCurrentUserId();

            var dto = new MarkNotificationAsReadDto
            {
                NotificationId = notificationId
            };

            var response = await _notificationService.MarkAsReadAsync(userId, dto);
            return Ok(response);
        }

        [HttpPut("read-all")]
        [Authorize(Roles = "Patient,Doctor,Admin")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();

            var response = await _notificationService.MarkAllAsReadAsync(userId);
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
    }
}
