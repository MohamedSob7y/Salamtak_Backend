namespace Salamtak.Shared.DTOs.Doctors;

public class DoctorSearchRequestDto
{
    public Guid? SpecialtyId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? MaxDistanceKm { get; set; }
    public int PageNumber { get; set; } = 1; public int PageSize { get; set; } = 10;
}
