using System.ComponentModel.DataAnnotations;

namespace Salamtak.Shared.DTOs.DoctorDocuments;

public class RejectDoctorDocumentDto
{
    [Required]
    [MaxLength(500)]
    public string RejectionReason { get; set; } = null!;
}
