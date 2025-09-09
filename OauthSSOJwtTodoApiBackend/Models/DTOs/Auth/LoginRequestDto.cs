namespace OauthSSOJwtTodoApiBackend.Models.DTOs.Auth;

/// <summary>
/// Incoming login request with username/email and password.
/// </summary>
public class LoginRequestDto
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

