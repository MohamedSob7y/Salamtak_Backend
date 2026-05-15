namespace Salamtak.Shared.DTOs.Specialties;

public class SpecialtyDto
{
    public Guid SpecialtyId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}
