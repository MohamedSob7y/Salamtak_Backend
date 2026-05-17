namespace Salamtak.Shared.DTOs.Feedbacks;

public class FeedbackDto
{
    public Guid FeedbackId { get; set; }

    public Guid PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public Guid DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public Guid AppointmentId { get; set; }

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}