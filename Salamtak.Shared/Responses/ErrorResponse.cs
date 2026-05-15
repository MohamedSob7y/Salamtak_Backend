namespace Salamtak.Shared.Responses;

public class ErrorResponse
{
    public bool Success { get; set; } = false;

    public string Message { get; set; } = null!;

    public int StatusCode { get; set; }

    public List<string> Errors { get; set; } = new();
}
