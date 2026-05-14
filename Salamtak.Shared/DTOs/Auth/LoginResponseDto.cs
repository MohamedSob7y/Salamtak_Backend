namespace Salamtak.Shared.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public CurrentUserDto User { get; set; } = new();
}