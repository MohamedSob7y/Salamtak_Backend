namespace Salamtak.Shared.DTOs.AI;

public class AiDoctorRecommendationDto
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public double Rating { get; set; }
    public decimal ConsultationFee { get; set; }
    public string Reason { get; set; } = string.Empty;
}
