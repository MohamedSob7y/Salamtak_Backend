namespace Salamtak.Shared.DTOs.DoctorDocuments;

public class RejectDoctorDocumentDto
{
    public Guid DocumentId { get; set; }

    public Guid AdminId { get; set; }

    public string RejectionReason { get; set; } = null!;
}
