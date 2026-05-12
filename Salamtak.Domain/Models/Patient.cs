using Salamtak.Domain.Models.Common_Entity;
using Salamtak.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models
{
    public class Patient : BaseEntity
    {
        public Guid UserId { get; set; }

        public Gender Gender { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string? Address { get; set; }

        public double? Height { get; set; }

        public double? Weight { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;

        public MedicalReport? MedicalReport { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();

        public ICollection<Feedback> Feedbacks { get; set; } = new HashSet<Feedback>();
    }
}
