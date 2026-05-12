using Salamtak.Domain.Models.Common_Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models
{
    public class Clinic : BaseEntity
    {
        public Guid DoctorId { get; set; }

        public string Name { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string City { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        // Navigation Properties
        public Doctor Doctor { get; set; } = null!;

        public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new HashSet<AvailabilitySlot>();

        public ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();
    }
}
