namespace Salamtak.Shared.DTOs.Admin;

public class DoctorVerificationRequestDto
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
}
