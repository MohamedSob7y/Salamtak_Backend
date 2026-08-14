using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.Constants;
using Salamtak.Shared.DTOs.Notifications;
using System;
using System.Threading.Tasks;

namespace Salamtak.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : BaseApiController
    {
        private readonly INotificationService
            _notificationService;

        public NotificationController(
            INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        
        [HttpPost("admin/send")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Create(
            [FromBody] CreateNotificationDto dto)
        {
            var response =
                await _notificationService.CreateAsync(dto);

            return Ok(response);
        }

      
        [HttpGet("me")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var currentUserId = GetCurrentUserId();

            var response = await _notificationService
                .GetUserNotificationsAsync(currentUserId);

            return Ok(response);
        }

       
        [HttpGet("admin/all")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAllNotifications()
        {
            var response = await _notificationService
                .GetAllNotificationsAsync();

            return Ok(response);
        }

       
        [HttpPut("me/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(
            [FromBody] MarkNotificationAsReadDto dto)
        {
            var currentUserId = GetCurrentUserId();

            var response = await _notificationService
                .MarkAsReadAsync(currentUserId, dto);

            return Ok(response);
        }

        [HttpPut("me/mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var currentUserId = GetCurrentUserId();

            var response = await _notificationService
                .MarkAllAsReadAsync(currentUserId);

            return Ok(response);
        }
    }
}