namespace Salamtak.Shared.DTOs.Admin;

public class UpdateUserStatusDto
{
    public Guid UserId { get; set; }

    public string Status { get; set; } = null!;
}
