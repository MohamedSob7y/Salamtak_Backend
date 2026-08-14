using Salamtak.Domain.Models.Common_Entity;
using Salamtak.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }

        public Guid? AppointmentId { get; set; }

        public string Title { get; set; } = null!;

        public string Message { get; set; } = null!;

        public NotificationType Type { get; set; }

        public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;

        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        public bool IsRead { get; set; } = false;

        public DateTime? SentAt { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;

        public Appointment? Appointment { get; set; }
    }
}
