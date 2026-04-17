namespace Application.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = [];
    public DateTime ExpiresAt { get; set; }
}
