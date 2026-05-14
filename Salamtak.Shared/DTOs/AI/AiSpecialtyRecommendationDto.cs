namespace Salamtak.Shared.DTOs.AI;

public class AiSpecialtyRecommendationDto
{
    public int SpecialtyId { get; set; }
    public string SpecialtyName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
}
