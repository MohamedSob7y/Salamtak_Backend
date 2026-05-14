namespace Salamtak.Shared.DTOs.DoctorDocuments;

public class RejectDoctorDocumentDto
{
    public int DocumentId { get; set; }
    public int AdminId { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}
