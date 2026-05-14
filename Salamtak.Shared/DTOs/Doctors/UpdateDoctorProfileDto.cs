namespace Salamtak.Shared.DTOs.Doctors;

public class UpdateDoctorProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int SpecialtyId { get; set; }
    public string Bio { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
}
