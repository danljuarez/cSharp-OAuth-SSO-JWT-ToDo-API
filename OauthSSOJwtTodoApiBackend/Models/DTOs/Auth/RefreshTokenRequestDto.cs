namespace OauthSSOJwtTodoApiBackend.Models.DTOs.Auth;

/// <summary>
/// Request to refresh an expired access token.
/// </summary>
public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
