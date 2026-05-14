namespace Salamtak.Shared.DTOs.MedicalReports;

public class CreateMedicalReportEntryDto
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
