namespace Salamtak.Shared.DTOs.Auth;

public class RegisterDoctorRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid SpecialtyId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    
    public decimal ConsultationFee { get; set; }
    public int ExperienceYears { get; set; }
}
