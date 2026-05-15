namespace Salamtak.Shared.DTOs.Feedbacks;

public class UpdateFeedbackDto
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
