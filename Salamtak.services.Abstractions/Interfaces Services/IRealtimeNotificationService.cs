using Salamtak.Shared.DTOs.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IRealtimeNotificationService
    {
        Task SendToUserAsync(Guid userId, RealtimeNotificationDto dto);

        Task SendToDoctorAsync(Guid doctorUserId, RealtimeNotificationDto dto);

        Task SendToPatientAsync(Guid patientUserId, RealtimeNotificationDto dto);

        Task SendToAdminAsync(Guid adminUserId, RealtimeNotificationDto dto);
    }
}
