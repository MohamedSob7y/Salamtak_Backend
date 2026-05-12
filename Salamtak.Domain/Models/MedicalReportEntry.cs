using Salamtak.Domain.Models.Common_Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models
{
    public class MedicalReportEntry : BaseEntity
    {
        public Guid MedicalReportId { get; set; }

        public Guid AppointmentId { get; set; }

        public Guid DoctorId { get; set; }

        public string? Diagnosis { get; set; }

        public string? Recommendations { get; set; }

        public string? Notes { get; set; }

        // Navigation Properties
        public MedicalReport MedicalReport { get; set; } = null!;

        public Appointment Appointment { get; set; } = null!;

        public Doctor Doctor { get; set; } = null!;

        public ICollection<Prescription> Prescriptions { get; set; } = new HashSet<Prescription>();
    }
}
