namespace Salamtak.Shared.DTOs.Clinics;

public class ClinicDto
{
    public Guid ClinicId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public DateTime CreatedAt { get; set; }
}
