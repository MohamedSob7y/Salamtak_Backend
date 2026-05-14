namespace Salamtak.Shared.DTOs.AvailabilitySlots;

public class CreateAvailabilitySlotDto
{
    public int DoctorId { get; set; }
    public int ClinicId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
