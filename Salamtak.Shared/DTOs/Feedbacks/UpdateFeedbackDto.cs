namespace Salamtak.Shared.DTOs.Feedbacks;

public class UpdateFeedbackDto
{
    public Guid FeedbackId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }
}
