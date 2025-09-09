namespace OauthSSOJwtTodoApiBackend.Models.DTOs.Auth;

/// <summary>
/// DTO used to exchange authorization code from LinkedIn using PKCE.
/// </summary>
public class LinkedInExchangeDto
{
    public string Code { get; set; } = string.Empty;
    public string CodeVerifier { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
