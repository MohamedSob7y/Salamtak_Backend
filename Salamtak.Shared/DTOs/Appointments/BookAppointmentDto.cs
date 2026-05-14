namespace Salamtak.Shared.DTOs.Appointments;

public class BookAppointmentDto
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int AvailabilitySlotId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
