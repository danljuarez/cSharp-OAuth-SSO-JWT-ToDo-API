namespace OauthSSOJwtTodoApiBackend.Models.DTOs.Auth;

/// <summary>
/// DTO representing the token response received from LinkedIn's OAuth2 token exchange endpoint.
/// </summary>
public class LinkedInTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}
