namespace Salamtak.Shared.DTOs.DoctorDocuments;

public class DoctorDocumentDto
{
    public Guid DocumentId { get; set; }

    public Guid DoctorId { get; set; }

    public string DoctorName { get; set; } = null!;

    public string DocumentType { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long FileSize { get; set; }

    public string? Description { get; set; }

    public bool IsVerified { get; set; }

    public string? RejectionReason { get; set; }

    public Guid? VerifiedByAdminId { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public DateTime UploadedAt { get; set; }
}
