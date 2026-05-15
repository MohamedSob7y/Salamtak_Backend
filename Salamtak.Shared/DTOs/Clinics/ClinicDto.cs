namespace Salamtak.Shared.DTOs.Clinics;

public class ClinicDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
