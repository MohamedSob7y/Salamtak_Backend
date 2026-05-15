using Salamtak.Shared.DTOs.Notifications;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface INotificationService
    {
        Task<ApiResponse<NotificationDto>> CreateAsync(CreateNotificationDto dto);

        Task<ApiResponse<IReadOnlyList<NotificationDto>>> GetUserNotificationsAsync(Guid userId);

        Task<ApiResponse> MarkAsReadAsync(Guid userId, MarkNotificationAsReadDto dto);

        Task<ApiResponse> MarkAllAsReadAsync(Guid userId);
    }
}
