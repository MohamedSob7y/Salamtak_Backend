namespace Salamtak.Shared.DTOs.AvailabilitySlots;

public class AvailabilitySlotDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public int ClinicId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsBooked { get; set; }
}
