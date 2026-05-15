namespace Salamtak.Shared.DTOs.Prescriptions;

public class CreatePrescriptionDto
{
    public string DrugName { get; set; } = null!;

    public string? Dose { get; set; }

    public string? Duration { get; set; }

    public string? Instructions { get; set; }
}
