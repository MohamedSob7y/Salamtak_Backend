namespace Salamtak.Shared.DTOs.MedicalReports;

public class PatientMedicalHistoryDto
{
    public Guid PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public List<MedicalReportEntryDto> MedicalReports { get; set; } = new();
}