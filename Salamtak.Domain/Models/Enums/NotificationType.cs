using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models.Enums
{
    public enum NotificationType
    {
        General = 0,

        AppointmentBooked = 1,

        AppointmentCancelled = 2,

        AppointmentCompleted = 3,

        DoctorVerified = 4,

        DoctorRejected = 5,

        MedicalReportUpdated = 6,

        PrescriptionAdded = 7
    }
}
