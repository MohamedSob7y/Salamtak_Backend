namespace Salamtak.Shared.DTOs.Doctors;

public class DoctorRatingSummaryDto
{
    public Guid DoctorId { get; set; }

    public double AverageRating { get; set; }

    public int TotalReviews { get; set; }
}