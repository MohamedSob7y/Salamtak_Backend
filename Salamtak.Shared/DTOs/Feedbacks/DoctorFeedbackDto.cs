namespace Salamtak.Shared.DTOs.Feedbacks;

public class DoctorFeedbackDto
{
    public int FeedbackId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
