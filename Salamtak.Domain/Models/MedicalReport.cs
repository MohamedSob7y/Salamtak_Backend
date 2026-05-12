using Salamtak.Domain.Models.Common_Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models
{

    public class MedicalReport : BaseEntity
    {
        public Guid PatientId { get; set; }

        // Navigation Properties
        public Patient Patient { get; set; } = null!;

        public ICollection<MedicalReportEntry> Entries { get; set; } = new HashSet<MedicalReportEntry>();
    }
}
