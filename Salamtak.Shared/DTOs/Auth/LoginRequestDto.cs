namespace Salamtak.Shared.DTOs.Auth;

public class LoginRequestDto
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;
}
