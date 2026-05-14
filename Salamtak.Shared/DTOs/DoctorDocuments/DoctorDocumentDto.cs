namespace Salamtak.Shared.DTOs.DoctorDocuments;

public class DoctorDocumentDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}
