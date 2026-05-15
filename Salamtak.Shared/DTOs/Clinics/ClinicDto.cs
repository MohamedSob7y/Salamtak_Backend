namespace Salamtak.Shared.DTOs.Clinics;

public class ClinicDto
{
    public Guid ClinicId { get; set; }

    public Guid DoctorId { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string City { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }
}
