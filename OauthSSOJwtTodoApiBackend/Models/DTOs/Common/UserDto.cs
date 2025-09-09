namespace OauthSSOJwtTodoApiBackend.Models.DTOs.Common;

/// <summary>
/// DTO representing a simplified user object exposed to the client.
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
}
