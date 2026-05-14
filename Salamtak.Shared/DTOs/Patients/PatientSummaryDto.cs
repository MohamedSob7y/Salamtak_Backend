namespace Salamtak.Shared.DTOs.Patients;

public class PatientSummaryDto
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
