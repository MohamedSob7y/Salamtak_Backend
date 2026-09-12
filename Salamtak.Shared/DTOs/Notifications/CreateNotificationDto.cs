namespace Salamtak.Shared.DTOs.Notifications
{
    public class CreateNotificationDto
    {
        public Guid UserId { get; set; }

        public Guid? AppointmentId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;
    }
}