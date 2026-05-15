namespace Salamtak.Shared.DTOs.Feedbacks;

public class CreateFeedbackDto
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int AppointmentId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
