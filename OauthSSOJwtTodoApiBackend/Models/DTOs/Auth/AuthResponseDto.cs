namespace OauthSSOJwtTodoApiBackend.Models.DTOs.Auth;

/// <summary>
/// DTO representing the authentication response payload returned after successful login.
/// </summary>
public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
