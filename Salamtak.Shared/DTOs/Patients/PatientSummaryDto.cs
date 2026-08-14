namespace Salamtak.Shared.DTOs.Patients;

public class PatientSummaryDto
{
    public Guid PatientId { get; set; }

    public string FullName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Gender { get; set; } = null!;
}
