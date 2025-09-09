namespace OauthSSOJwtTodoApiBackend.Models.DTOs.Auth;

/// <summary>
/// Successful login response containing access and refresh tokens.
/// </summary>
public class LoginResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }        // In seconds
}
