using Salamtak.Domain.Models.Common_Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models
{
    public class Feedback : BaseEntity
    {
        public Guid PatientId { get; set; }

        public Guid DoctorId { get; set; }

        public Guid AppointmentId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        // Navigation Properties
        public Patient Patient { get; set; } = null!;

        public Doctor Doctor { get; set; } = null!;

        public Appointment Appointment { get; set; } = null!;
    }
}
