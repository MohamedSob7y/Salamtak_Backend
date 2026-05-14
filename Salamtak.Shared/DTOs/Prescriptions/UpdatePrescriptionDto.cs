namespace Salamtak.Shared.DTOs.Prescriptions;

public class UpdatePrescriptionDto
{
    public int Id { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
}
