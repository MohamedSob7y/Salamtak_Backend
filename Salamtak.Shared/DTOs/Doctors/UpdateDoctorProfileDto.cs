namespace Salamtak.Shared.DTOs.Doctors;

public class UpdateDoctorProfileDto
{
    public string FullName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public Guid SpecialtyId { get; set; }

    public string? Bio { get; set; }

    public decimal ConsultationFee { get; set; }

    public int ExperienceYears { get; set; }
}
