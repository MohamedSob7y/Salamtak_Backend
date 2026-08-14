namespace Salamtak.Shared.DTOs.Users;

public class UserSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
