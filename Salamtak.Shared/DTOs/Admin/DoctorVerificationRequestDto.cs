namespace Salamtak.Shared.DTOs.Admin;

public class DoctorVerificationRequestDto
{
    public Guid DoctorId { get; set; }

    public string DoctorName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string SpecialtyName { get; set; } = null!;

    public string VerificationStatus { get; set; } = null!;

    public int DocumentsCount { get; set; }
}
