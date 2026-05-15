namespace Salamtak.Shared.DTOs.Appointments;

public class PatientAppointmentDto
{
    public int AppointmentId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
