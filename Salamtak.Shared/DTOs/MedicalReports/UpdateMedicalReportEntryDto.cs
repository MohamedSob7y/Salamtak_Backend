namespace Salamtak.Shared.DTOs.MedicalReports;

public class UpdateMedicalReportEntryDto
{
    public int Id { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
