namespace Salamtak.Shared.DTOs.MedicalReports;

public class MedicalReportDto
{
    public Guid MedicalReportId { get; set; }

    public Guid PatientId { get; set; }

    public string PatientName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<MedicalReportEntryDto> Entries { get; set; } = new();
}
