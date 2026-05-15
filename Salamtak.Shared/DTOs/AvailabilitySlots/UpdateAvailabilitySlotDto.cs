namespace Salamtak.Shared.DTOs.AvailabilitySlots;

public class UpdateAvailabilitySlotDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsBooked { get; set; }
}
