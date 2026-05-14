namespace Salamtak.Shared.DTOs.Admin;

public class DoctorVerificationResultDto
{
    public int DoctorId { get; set; }
    public bool IsApproved { get; set; }
    public string Message { get; set; } = string.Empty;
}
