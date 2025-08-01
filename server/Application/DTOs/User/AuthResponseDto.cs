namespace Application.DTOs.User;

public class AuthResponseDto
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }

    public Guid Id { get; init; }
    public string UserName { get; init; }

}
