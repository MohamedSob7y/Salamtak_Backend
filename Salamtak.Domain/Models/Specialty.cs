using Salamtak.Domain.Models.Common_Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models
{
    public class Specialty : BaseEntity
    {
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        // Navigation Properties
        public ICollection<Doctor> Doctors { get; set; } = new HashSet<Doctor>();
    }
}
