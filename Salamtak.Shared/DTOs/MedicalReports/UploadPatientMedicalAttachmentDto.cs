using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
namespace Salamtak.Shared.DTOs.MedicalReports
{
    public class UploadPatientMedicalAttachmentDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
