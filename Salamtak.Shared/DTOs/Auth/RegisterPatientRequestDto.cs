namespace Salamtak.Shared.DTOs.Auth;

public class RegisterPatientRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Address { get; set; }

    public string? BloodType { get; set; }

    public double? Height { get; set; }

    public double? Weight { get; set; }
}
