namespace Salamtak.Shared.DTOs.Auth;

public class CurrentUserDto
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;
}
