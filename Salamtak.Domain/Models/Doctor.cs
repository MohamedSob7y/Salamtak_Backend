using Salamtak.Domain.Models.Common_Entity;
using Salamtak.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models
{
    public class Doctor : BaseEntity
    {
        public Guid UserId { get; set; }

        public Guid SpecialtyId { get; set; }

        public string? Bio { get; set; }

        public int ExperienceYears { get; set; }

        public string? LicenseNumber { get; set; }

        public DoctorVerificationStatus VerificationStatus { get; set; } = DoctorVerificationStatus.Pending;

        public bool IsVerified { get; set; } = false;

        public double AverageRating { get; set; } = 0;

        // Navigation Properties
        public User User { get; set; } = null!;

        public Specialty Specialty { get; set; } = null!;

        public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new HashSet<AvailabilitySlot>();

        public ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();

        public ICollection<MedicalReportEntry> MedicalReportEntries { get; set; } = new HashSet<MedicalReportEntry>();
        public ICollection<Clinic> Clinics { get; set; } = new HashSet<Clinic>();
        public ICollection<DoctorDocument> DoctorDocuments { get; set; } = new HashSet<DoctorDocument>();

        public ICollection<Feedback> Feedbacks { get; set; } = new HashSet<Feedback>();
    }
}
