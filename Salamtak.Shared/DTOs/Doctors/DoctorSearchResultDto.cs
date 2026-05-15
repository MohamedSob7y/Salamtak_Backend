namespace Salamtak.Shared.DTOs.Doctors;

public class DoctorSearchResultDto
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public double Rating { get; set; }
    public bool IsVerified { get; set; }
}
