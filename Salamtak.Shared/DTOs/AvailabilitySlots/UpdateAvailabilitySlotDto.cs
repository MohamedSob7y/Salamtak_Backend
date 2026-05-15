namespace Salamtak.Shared.DTOs.AvailabilitySlots;

public class UpdateAvailabilitySlotDto
{
    public Guid SlotId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsAvailable { get; set; }
}
