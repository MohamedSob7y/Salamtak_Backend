namespace Salamtak.Shared.DTOs.Doctors;

public class DoctorDetailsDto
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public double Rating { get; set; }
    public int ReviewsCount { get; set; }
    public bool IsVerified { get; set; }
}
