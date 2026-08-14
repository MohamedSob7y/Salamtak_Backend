namespace Salamtak.Shared.DTOs.Doctors;

public class DoctorCardDto
{
    public Guid DoctorId { get; set; }

    public Guid SpecialtyId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string SpecialtyName { get; set; } = string.Empty;

    public Guid? NearestClinicId { get; set; }

    public string? NearestClinicName { get; set; }

    public string? City { get; set; }

    public string? ClinicAddress { get; set; }

    public double? DistanceKm { get; set; }

    public double AverageRating { get; set; }

    public int ReviewsCount { get; set; }

    public decimal ConsultationFee { get; set; }

    public bool IsVerified { get; set; }
}
