namespace Salamtak.Shared.DTOs.Notifications;

public class NotificationDto
{
    public Guid NotificationId { get; set; }

    public Guid UserId { get; set; }

    public Guid? AppointmentId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Channel { get; set; } = null!;

    public string Status { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? SentAt { get; set; }
}
