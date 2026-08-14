namespace Salamtak.Shared.DTOs.Notifications;

public class NotificationDto
{
    public Guid NotificationId { get; set; }

    public Guid UserId { get; set; }

    public string RecipientName { get; set; } = string.Empty;

    public string RecipientEmail { get; set; } = string.Empty;

    public string RecipientRole { get; set; } = string.Empty;

    public Guid? AppointmentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Channel { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? SentAt { get; set; }
}
