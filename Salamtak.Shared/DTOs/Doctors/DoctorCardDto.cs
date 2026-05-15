namespace Salamtak.Shared.DTOs.Doctors;

public class DoctorCardDto
{
    public Guid DoctorId { get; set; }

    public string FullName { get; set; } = null!;

    public string SpecialtyName { get; set; } = null!;

    public string? City { get; set; }

    public string? ClinicAddress { get; set; }

    public double AverageRating { get; set; }

    public int ReviewsCount { get; set; }

    public decimal ConsultationFee { get; set; }

    public bool IsVerified { get; set; }
}
