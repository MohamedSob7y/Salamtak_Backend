using Microsoft.AspNetCore.SignalR;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.Notifications;
using Salamtak.Web.Api.Hubs_Real_Time;

namespace Salamtak.Web.Api.Realtime
{
    public class SignalRRealtimeNotificationService : IRealtimeNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRRealtimeNotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToUserAsync(Guid userId, RealtimeNotificationDto dto)
        {
            await _hubContext.Clients
                .User(userId.ToString())
                .SendAsync("ReceiveNotification", dto);
        }

        public Task SendToDoctorAsync(Guid doctorUserId, RealtimeNotificationDto dto)
        {
            return SendToUserAsync(doctorUserId, dto);
        }

        public Task SendToPatientAsync(Guid patientUserId, RealtimeNotificationDto dto)
        {
            return SendToUserAsync(patientUserId, dto);
        }

        public Task SendToAdminAsync(Guid adminUserId, RealtimeNotificationDto dto)
        {
            return SendToUserAsync(adminUserId, dto);
        }
    }
}
