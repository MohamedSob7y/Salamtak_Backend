namespace Salamtak.Shared.DTOs.MedicalReports;

public class MedicalReportEntryDto
{
    public int Id { get; set; }
    public int MedicalReportId { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
