namespace Salamtak.Shared.DTOs.Clinics;

public class ClinicDto
{
    public Guid ClinicId { get; set; }

    public Guid DoctorId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
