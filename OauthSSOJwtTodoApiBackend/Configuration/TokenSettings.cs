namespace OauthSSOJwtTodoApiBackend.Configuration;

/// <summary>
/// Settings for controlling token expiration times.
/// </summary>
public class TokenSettings
{
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
