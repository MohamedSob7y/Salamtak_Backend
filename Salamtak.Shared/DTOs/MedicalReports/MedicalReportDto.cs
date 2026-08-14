namespace Salamtak.Shared.DTOs.MedicalReports;

public class MedicalReportDto
{
    public Guid MedicalReportId { get; set; }

    public Guid PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    
    public List<MedicalReportAttachmentDto> Attachments
    {
        get;
        set;
    } = new();

    public List<MedicalReportEntryDto> Entries
    {
        get;
        set;
    } = new();
}
