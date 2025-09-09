namespace OauthSSOJwtTodoApiBackend.Models.DTOs.Auth;

/// <summary>
/// DTO representing the authentication request payload containing user credentials.
/// </summary>
public class AuthRequestDto
{
    public string Identifier { get; set; } = string.Empty; // Username or Email
    public string Password { get; set; } = string.Empty;
}
