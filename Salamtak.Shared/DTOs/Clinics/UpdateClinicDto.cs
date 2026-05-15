namespace Salamtak.Shared.DTOs.Clinics;

public class UpdateClinicDto
{
    public Guid ClinicId { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string City { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }
}
