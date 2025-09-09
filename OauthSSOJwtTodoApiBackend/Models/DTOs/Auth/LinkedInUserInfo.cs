namespace OauthSSOJwtTodoApiBackend.Models.DTOs.Auth;

/// <summary>
/// DTO representing basic user profile information retrieved from LinkedIn's User Info endpoint.
/// </summary>
public class LinkedInUserInfo
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
}
