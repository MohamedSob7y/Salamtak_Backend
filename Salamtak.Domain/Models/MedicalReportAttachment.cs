using Salamtak.Domain.Models.Common_Entity;
using Salamtak.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models
{
    public class MedicalReportAttachment:BaseEntity
    {
        
        public Guid MedicalReportId { get; set; }

       
        public Guid? MedicalReportEntryId { get; set; }

       
        public Guid UploadedByUserId { get; set; }

        public AttachmentUploaderType UploadedByType { get; set; }

       
        public string OriginalFileName { get; set; } = null!;

       
        public string StoredFileName { get; set; } = null!;

        
        public string StoragePath { get; set; } = null!;

        
        public string ContentType { get; set; } = null!;

        public long FileSize { get; set; }

        public string? Description { get; set; }

        
        public MedicalReport MedicalReport { get; set; } = null!;

        public MedicalReportEntry? MedicalReportEntry { get; set; }

        public User UploadedByUser { get; set; } = null!;
    }
}
