namespace Salamtak.Shared.DTOs.AvailabilitySlots;

public class CreateAvailabilitySlotDto
{
    public Guid ClinicId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }
}
