using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Salamtak.Shared.DTOs.DoctorDocuments;

public class UploadDoctorDocumentDto
{
    [Required]
    public string DocumentType { get; set; } = null!;

    [Required]
    public IFormFile File { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }
}
