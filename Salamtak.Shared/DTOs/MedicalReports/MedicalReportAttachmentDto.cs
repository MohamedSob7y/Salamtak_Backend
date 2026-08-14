using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Shared.DTOs.MedicalReports
{
    public class MedicalReportAttachmentDto
    {
        public Guid Id { get; set; }

        public Guid MedicalReportId { get; set; }

        public Guid? MedicalReportEntryId { get; set; }

        public Guid UploadedByUserId { get; set; }

        public string UploadedByType { get; set; } = null!;

        public string OriginalFileName { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public long FileSize { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
