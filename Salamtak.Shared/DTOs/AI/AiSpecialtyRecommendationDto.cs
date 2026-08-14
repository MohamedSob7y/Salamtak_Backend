namespace Salamtak.Shared.DTOs.AI;

public class AiSpecialtyRecommendationDto
{
    public Guid? SpecialtyId { get; set; }

    public string SpecialtyName { get; set; } = null!;

    public double? Confidence { get; set; }
}
