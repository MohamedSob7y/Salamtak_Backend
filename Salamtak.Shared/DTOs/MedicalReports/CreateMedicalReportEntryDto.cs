using Salamtak.Shared.DTOs.Prescriptions;

namespace Salamtak.Shared.DTOs.MedicalReports;

public class CreateMedicalReportEntryDto
{
    public Guid AppointmentId { get; set; }

    public string? Diagnosis { get; set; }

    public string? Recommendations { get; set; }

    public string? Notes { get; set; }

    public List<CreatePrescriptionDto> Prescriptions { get; set; } = new();
}
