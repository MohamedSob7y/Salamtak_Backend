using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models.Enums
{
    public enum NotificationType
    {
        AppointmentBooked = 1,
        AppointmentCancelled = 2,
        AppointmentReminder = 3,
        DoctorVerified = 4,
        DoctorRejected = 5,
        MedicalReportUpdated = 6,
        NewFeedback = 7
    }
}
