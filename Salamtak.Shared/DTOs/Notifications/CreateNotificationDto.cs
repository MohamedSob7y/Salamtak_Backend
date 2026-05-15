namespace Salamtak.Shared.DTOs.Notifications;

public class CreateNotificationDto
{
    public Guid UserId { get; set; }

    public Guid? AppointmentId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Channel { get; set; } = "InApp";
}
