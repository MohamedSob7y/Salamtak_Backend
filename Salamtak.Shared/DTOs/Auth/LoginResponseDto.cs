namespace Salamtak.Shared.DTOs.Auth;

public class LoginResponseDto
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string Token { get; set; } = null!;

    public DateTime Expiration { get; set; }
}
