using Salamtak.Domain.Models.Common_Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models
{
    public class Prescription : BaseEntity
    {
        public Guid MedicalReportEntryId { get; set; }

        public string DrugName { get; set; } = null!;

        public string? Dose { get; set; }

        public string? Duration { get; set; }

        public string? Instructions { get; set; }

        // Navigation Properties
        public MedicalReportEntry MedicalReportEntry { get; set; } = null!;
    }
}
