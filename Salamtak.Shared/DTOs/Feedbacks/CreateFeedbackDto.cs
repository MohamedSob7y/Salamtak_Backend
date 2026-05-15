namespace Salamtak.Shared.DTOs.Feedbacks;

public class CreateFeedbackDto
{
    public Guid AppointmentId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }
}
