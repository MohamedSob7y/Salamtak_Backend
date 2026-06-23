using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.Notifications;

namespace Salamtak.Web.Api.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            var response = await _notificationService.CreateAsync(dto);
            return Ok(response);
        }

        [HttpGet("users/{userId:guid}")]
        public async Task<IActionResult> GetUserNotifications(Guid userId)
        {
            var response = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(response);
        }

        [HttpPut("users/{userId:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid userId, [FromBody] MarkNotificationAsReadDto dto)
        {
            var response = await _notificationService.MarkAsReadAsync(userId, dto);
            return Ok(response);
        }

        [HttpPut("users/{userId:guid}/read-all")]
        public async Task<IActionResult> MarkAllAsRead(Guid userId)
        {
            var response = await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(response);
        }
    }
}
