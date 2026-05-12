using Salamtak.Domain.Models.Common_Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models
{
    public class AvailabilitySlot : BaseEntity
    {
        public Guid DoctorId { get; set; }

        public Guid ClinicId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Navigation Properties
        public Doctor Doctor { get; set; } = null!;

        public Clinic Clinic { get; set; } = null!;

        public Appointment? Appointment { get; set; }
    }
}
