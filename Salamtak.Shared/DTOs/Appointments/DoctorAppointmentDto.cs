namespace Salamtak.Shared.DTOs.Appointments;

public class DoctorAppointmentDto
{
    public Guid AppointmentId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
