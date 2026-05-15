namespace Salamtak.Shared.DTOs.Appointments;

public class BookAppointmentDto
{
    public Guid DoctorId { get; set; }

    public Guid ClinicId { get; set; }

    public Guid AvailabilitySlotId { get; set; }

    public string BookingMethod { get; set; } = "Direct";

    public string? Reason { get; set; }
}
