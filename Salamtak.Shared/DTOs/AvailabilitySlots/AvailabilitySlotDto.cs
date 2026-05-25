namespace Salamtak.Shared.DTOs.AvailabilitySlots;

public class AvailabilitySlotDto
{
    public Guid SlotId { get; set; }

    public Guid DoctorId { get; set; }

    public Guid ClinicId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsAvailable { get; set; }
} 
