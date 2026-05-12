using Salamtak.Domain.Models.Common_Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models
{
    public class Admin : BaseEntity
    {
        public Guid UserId { get; set; }

        public string? Department { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;

        public ICollection<DoctorDocument> VerifiedDoctorDocuments { get; set; } = new HashSet<DoctorDocument>();
    }
}
