namespace Salamtak.Shared.DTOs.AI;

public class AiSymptomRequestDto
{
    public int PatientId { get; set; }
    public string Symptoms { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string Gender { get; set; } = string.Empty;
}
