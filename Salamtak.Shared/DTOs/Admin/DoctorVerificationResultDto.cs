namespace Salamtak.Shared.DTOs.Admin;

public class DoctorVerificationResultDto
{

    public Guid DoctorId { get; set; }

    public bool IsApproved { get; set; }

    public string? RejectionReason { get; set; }
}
