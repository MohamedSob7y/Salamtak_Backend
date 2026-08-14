namespace Salamtak.Shared.DTOs.Appointments;

public class CancelAppointmentDto
{
    public Guid AppointmentId { get; set; }

    public string? CancelReason { get; set; }
}
