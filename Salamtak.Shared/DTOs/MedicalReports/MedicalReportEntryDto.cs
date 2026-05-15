using Salamtak.Shared.DTOs.Prescriptions;

namespace Salamtak.Shared.DTOs.MedicalReports;

public class MedicalReportEntryDto
{
    public Guid EntryId { get; set; }

    public Guid AppointmentId { get; set; }

    public Guid DoctorId { get; set; }

    public string DoctorName { get; set; } = null!;

    public string? Diagnosis { get; set; }

    public string? Recommendations { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<PrescriptionDto> Prescriptions { get; set; } = new();
}
