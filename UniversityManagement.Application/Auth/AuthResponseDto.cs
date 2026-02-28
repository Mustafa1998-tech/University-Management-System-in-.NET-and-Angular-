namespace UniversityManagement.Application.Auth;

public class AuthResponseDto
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public string Role { get; set; }
    public int UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
}
