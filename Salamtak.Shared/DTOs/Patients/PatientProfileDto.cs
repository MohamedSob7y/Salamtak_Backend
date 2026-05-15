namespace Salamtak.Shared.DTOs.Patients;

public class PatientProfileDto
{
    public Guid PatientId { get; set; }

    public Guid UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = null!;

    public string? Address { get; set; }

    public string? BloodType { get; set; }

    public double? Height { get; set; }

    public double? Weight { get; set; }
}
