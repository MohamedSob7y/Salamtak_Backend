namespace Salamtak.Shared.DTOs.Appointments;

public class CancelAppointmentDto
{
    public int AppointmentId { get; set; }
    public string CancellationReason { get; set; } = string.Empty;
}
