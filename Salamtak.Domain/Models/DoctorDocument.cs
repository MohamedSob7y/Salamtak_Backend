using Salamtak.Domain.Models.Common_Entity;
using Salamtak.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models
{
    public class DoctorDocument : BaseEntity
    {
        public Guid DoctorId { get; set; }

        public Guid? VerifiedByAdminId { get; set; }

        public DoctorDocumentType DocumentType { get; set; }

        public string FileUrl { get; set; } = null!;

        public bool IsVerified { get; set; } = false;

        public string? RejectionReason { get; set; }

        public DateTime? VerifiedAt { get; set; }

        // Navigation Properties
        public Doctor Doctor { get; set; } = null!;

        public Admin? VerifiedByAdmin { get; set; }
    }
}
