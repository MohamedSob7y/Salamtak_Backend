namespace Salamtak.Shared.DTOs.DoctorDocuments;

public class UploadDoctorDocumentDto
{
    public int DoctorId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
}
