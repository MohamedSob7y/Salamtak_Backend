namespace Salamtak.Shared.DTOs.Doctors;
public class DoctorDetailsDto
{
    public Guid DoctorId { get; set; }

    public Guid UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public Guid SpecialtyId { get; set; }

    public string SpecialtyName { get; set; } = null!;

    public string? Bio { get; set; }

    public int ExperienceYears { get; set; }

    public string? LicenseNumber { get; set; }

    public string VerificationStatus { get; set; } = null!;

    public bool IsVerified { get; set; }

    public double AverageRating { get; set; }

    public int ReviewsCount { get; set; }

    public decimal ConsultationFee { get; set; }
}
