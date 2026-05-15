namespace Salamtak.Shared.DTOs.DoctorDocuments;

public class UploadDoctorDocumentDto
{
    public Guid DoctorId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string FileUrl { get; set; } = null!;
}
