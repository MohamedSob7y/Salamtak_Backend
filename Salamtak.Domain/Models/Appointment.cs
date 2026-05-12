using Salamtak.Domain.Models.Common_Entity;
using Salamtak.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models
{
    public class Appointment : BaseEntity
    {
        public Guid PatientId { get; set; }

        public Guid DoctorId { get; set; }

        public Guid ClinicId { get; set; }

        public Guid AvailabilitySlotId { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public BookingMethod BookingMethod { get; set; } = BookingMethod.Direct;

        public string BookingCode { get; set; } = Guid.NewGuid().ToString("N")[..10].ToUpper();

        public string? CancelReason { get; set; }

        // Navigation Properties
        public Patient Patient { get; set; } = null!;

        public Doctor Doctor { get; set; } = null!;

        public Clinic Clinic { get; set; } = null!;

        public AvailabilitySlot AvailabilitySlot { get; set; } = null!;

        public MedicalReportEntry? MedicalReportEntry { get; set; }

        public Feedback? Feedback { get; set; }

        public ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();
    }

}
