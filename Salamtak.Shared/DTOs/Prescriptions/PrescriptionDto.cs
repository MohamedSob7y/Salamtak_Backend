namespace Salamtak.Shared.DTOs.Prescriptions;

public class PrescriptionDto
{
    public Guid PrescriptionId { get; set; }

    public Guid MedicalReportEntryId { get; set; }

    public string DrugName { get; set; } = null!;

    public string? Dose { get; set; }

    public string? Duration { get; set; }

    public string? Instructions { get; set; }
}
