namespace OauthSSOJwtTodoApiBackend.Models.DTOs.Auth;

/// <summary>
/// Wrapper for internally issued JWT tokens.
/// </summary>
public class JwtTokenResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }            // In seconds
}
