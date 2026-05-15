namespace Salamtak.Shared.DTOs.MedicalReports;

public class MedicalReportDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<MedicalReportEntryDto> Entries { get; set; } = new();
}
