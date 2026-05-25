namespace Salamtak.Shared.DTOs.AvailabilitySlots;

public class AvailableSlotSearchDto
{
    public Guid? DoctorId { get; set; }

    public Guid? SpecialtyId { get; set; }

    public Guid? ClinicId { get; set; }

    public DateTime? Date { get; set; }
}
