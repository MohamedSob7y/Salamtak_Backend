namespace Salamtak.Shared.DTOs.Auth;

public class RegisterDoctorRequestDto
{
    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Password { get; set; } = null!;

    public Guid SpecialtyId { get; set; }

    public string LicenseNumber { get; set; } = null!;

    public string? Bio { get; set; }

    public decimal ConsultationFee { get; set; }

    public int ExperienceYears { get; set; }
}
