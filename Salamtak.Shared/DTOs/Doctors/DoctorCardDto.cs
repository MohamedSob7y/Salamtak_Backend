namespace Salamtak.Shared.DTOs.Doctors;

public class DoctorCardDto
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public double Rating { get; set; }
}
