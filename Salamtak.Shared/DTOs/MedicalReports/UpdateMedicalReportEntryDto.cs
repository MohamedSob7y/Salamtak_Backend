using Salamtak.Shared.DTOs.Prescriptions;

namespace Salamtak.Shared.DTOs.MedicalReports;

public class UpdateMedicalReportEntryDto
{
    public Guid EntryId { get; set; }

    public string? Diagnosis { get; set; }

    public string? Recommendations { get; set; }

    public string? Notes { get; set; }

    public List<UpdatePrescriptionDto> Prescriptions { get; set; } = new();
}
