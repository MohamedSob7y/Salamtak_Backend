namespace Salamtak.Shared.DTOs.Doctors;

public class DoctorProfileDto
{
    public Guid DoctorId { get; set; }

    public Guid UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public Guid SpecialtyId { get; set; }

    public string SpecialtyName { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public string? LicenseNumber { get; set; }

    public decimal ConsultationFee { get; set; }

    public bool IsVerified { get; set; }

    public int ExperienceYears { get; set; }
}