namespace OauthSSOJwtTodoApiBackend.Models.DTOs.Auth;

/// <summary>
/// Result of a successful token refresh operation.
/// </summary>
public class RefreshResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }        // In seconds
}
