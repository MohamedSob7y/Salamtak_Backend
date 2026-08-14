using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Shared.DTOs.MedicalReports
{
    public class UploadDoctorMedicalAttachmentDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
