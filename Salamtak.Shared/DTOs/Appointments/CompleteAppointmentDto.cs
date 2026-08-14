namespace Salamtak.Shared.DTOs.Appointments;

public class CompleteAppointmentDto
{
    public Guid AppointmentId { get; set; }
    public string Notes { get; set; } = string.Empty;
}
