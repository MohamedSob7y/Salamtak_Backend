namespace Salamtak.Shared.DTOs.Doctors;

public class DoctorSearchRequestDto
{
    public string? DoctorName { get; set; }

    public Guid? SpecialtyId { get; set; }

    public string? City { get; set; }

    public decimal? MinFee { get; set; }

    public decimal? MaxFee { get; set; }

    public double? MinRating { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
