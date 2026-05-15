namespace Salamtak.Shared.DTOs.Patients;

public class UpdatePatientProfileDto
{
    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = null!;

    public string? Address { get; set; }

    public string? BloodType { get; set; }

    public double? Height { get; set; }

    public double? Weight { get; set; }
}
