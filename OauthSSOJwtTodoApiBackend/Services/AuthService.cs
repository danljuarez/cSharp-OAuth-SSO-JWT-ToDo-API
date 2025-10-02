using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OauthSSOJwtTodoApiBackend.Configuration;
using OauthSSOJwtTodoApiBackend.Data;
using OauthSSOJwtTodoApiBackend.Enums;
using OauthSSOJwtTodoApiBackend.Extensions;
using OauthSSOJwtTodoApiBackend.Helpers;
using OauthSSOJwtTodoApiBackend.Models.DTOs.Auth;
using OauthSSOJwtTodoApiBackend.Models.Entities;
using System.Text.Json;

namespace OauthSSOJwtTodoApiBackend.Services;

public class AuthService : IAuthService
{
    private readonly TodoDbContext _db;
    private readonly IJwtHelper _jwtHelper;
    private readonly ILogger<AuthService> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly TokenSettings _tokenSettings;
    private readonly HttpClient _httpClient;

    public AuthService(
        TodoDbContext db,
        IJwtHelper jwtHelper,
        ILogger<AuthService> logger,
        IWebHostEnvironment env,
        IConfiguration config,
        IOptions<TokenSettings> tokenOptions,
        HttpClient httpClient)
    {
        _db = db;
        _jwtHelper = jwtHelper;
        _logger = logger;
        _env = env;
        _config = config;
        _tokenSettings = tokenOptions.Value;
        _httpClient = httpClient;
    }

    public async Task<(LoginResultDto? Data, AuthOperationResult Result)> LoginAsync(LoginRequestDto request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail);

        if (user is null || !PasswordHasher.VerifyPassword(request.Password, user.HashedPassword))
        {
            _logger.LogWarning("Login failed for {UsernameOrEmail}: invalid credentials", request.UsernameOrEmail);
            return (null, AuthOperationResult.InvalidCredentials);
        }

        if (string.IsNullOrWhiteSpace(user.Role) ||
            !(new[] { "User", "Manager", "Admin" }.Contains(user.Role)))
        {
            _logger.LogWarning("Login failed for {Username}: missing or invalid role", user.Username);
            return (null, AuthOperationResult.InvalidCredentials);
        }

        var accessToken = _jwtHelper.GenerateAccessToken(user);

        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_tokenSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow // Optional
        };

        await _db.RefreshTokens.AddAsync(refreshToken);
        await _db.SaveChangesAsync();

        _logger.LogInformation("User {Username} logged in with AccessToken: {AccessToken} and RefreshToken: {RefreshToken}",
            user.Username,
            _logger.TruncateToken(accessToken),
            _logger.TruncateToken(refreshToken.Token));

        return (new LoginResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = _tokenSettings.AccessTokenExpirationMinutes * 60
        }, AuthOperationResult.Success);
    }

    public async Task<(RefreshResultDto? Data, AuthOperationResult Result)> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.ExpiresAt > DateTime.UtcNow);

        if (storedToken is null || storedToken.User is null)
        {
            _logger.LogWarning("Refresh token failed: invalid or expired token {RefreshToken}", _logger.TruncateToken(refreshToken));
            return (null, AuthOperationResult.TokenInvalid);
        }

        // Rotate token
        storedToken.RevokedAt = DateTime.UtcNow; // Optional, for tracking
        _db.RefreshTokens.Remove(storedToken);

        var newToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = storedToken.User.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_tokenSettings.RefreshTokenExpirationDays)
        };

        await _db.RefreshTokens.AddAsync(newToken);
        await _db.SaveChangesAsync();

        var accessToken = _jwtHelper.GenerateAccessToken(storedToken.User);

        return (new RefreshResultDto
        {
            AccessToken = accessToken,
            RefreshToken = newToken.Token,
            ExpiresIn = _tokenSettings.AccessTokenExpirationMinutes * 60
        }, AuthOperationResult.Success);
    }

    public async Task<AuthOperationResult> LogoutAsync(string refreshToken)
    {
        var storedToken = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (storedToken != null)
        {
            storedToken.RevokedAt = DateTime.UtcNow; // Optional, for tracking
            _db.RefreshTokens.Remove(storedToken);
            await _db.SaveChangesAsync();

            _logger.LogTokenInfo("User with refresh token {Token} logged out", _logger.TruncateToken(refreshToken));
            return AuthOperationResult.Success;
        }

        _logger.LogWarning("Logout attempted with invalid refresh token {Token}", _logger.TruncateToken(refreshToken));
        return AuthOperationResult.LogoutInvalidToken;
    }

    public async Task<(JwtTokenResultDto? Data, AuthOperationResult Result)> ExchangeLinkedInCodeAsync(string code, string codeVerifier)
    {
        try
        {
            if (_env.IsDevelopment())
            {
                // Test flow for development
                var devUser = await _db.Users.FirstOrDefaultAsync();

                if (devUser == null)
                {
                    _logger.LogError("Dev LinkedIn login failed: dev user not found");
                    return (null, AuthOperationResult.UserNotFound);
                }

                var mockToken = _jwtHelper.GenerateAccessToken(devUser);

                _logger.LogInformation("Mock LinkedIn login succeeded for {Username}. AccessToken: {AccessToken}",
                    devUser.Username,
                    _logger.TruncateToken(mockToken));

                return (new JwtTokenResultDto
                {
                    AccessToken = mockToken,
                    ExpiresIn = _tokenSettings.AccessTokenExpirationMinutes * 60
                }, AuthOperationResult.Success);
            }

            // Real LinkedIn flow for Production
            var linkedInConfig = _config.GetSection("LinkedIn");
            var clientId = linkedInConfig["ClientId"] ?? throw new InvalidOperationException("LinkedIn ClientId is missing");
            var clientSecret = linkedInConfig["ClientSecret"] ?? throw new InvalidOperationException("LinkedIn ClientSecret is missing");
            var redirectUri = linkedInConfig["RedirectUri"] ?? throw new InvalidOperationException("LinkedIn RedirectUri is missing");
            var tokenEndpoint = linkedInConfig["TokenEndpoint"];
            var userInfoEndpoint = linkedInConfig["UserInfoEndpoint"];

            var values = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "code_verifier", codeVerifier } // PKCE
            };

            using var client = _httpClient;
            var tokenResponse = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(values));

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogError("LinkedIn token exchange failed. StatusCode: {StatusCode}", tokenResponse.StatusCode);
                return (null, AuthOperationResult.LinkedInExchangeFailed);
            }

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<LinkedInTokenResponse>(tokenJson)!;

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenData.AccessToken);

            var userInfoResponse = await client.GetAsync(userInfoEndpoint);

            if (!userInfoResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve LinkedIn user info. StatusCode: {StatusCode}", userInfoResponse.StatusCode);
                return (null, AuthOperationResult.LinkedInExchangeFailed);
            }

            var userJson = await userInfoResponse.Content.ReadAsStringAsync();
            var linkedInUser = JsonSerializer.Deserialize<LinkedInUserInfo>(userJson)!;

            // Find or create local user (you can expand this logic)
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == linkedInUser.EmailAddress);
            if (user is null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = linkedInUser.FirstName + linkedInUser.LastName,
                    Email = linkedInUser.EmailAddress,
                    Role = "User", // or default role
                    HashedPassword = "LinkedInSSO" // Placeholder; password not used
                };

                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync();

                _logger.LogInformation("New user created from LinkedIn: {Email}", user.Email);
            }
            else
            {
                _logger.LogInformation("LinkedIn login for existing user: {Email}", user.Email);
            }

            var accessToken = _jwtHelper.GenerateAccessToken(user);

            _logger.LogInformation("LinkedIn login successful. AccessToken: {AccessToken}", _logger.TruncateToken(accessToken));

            return (new JwtTokenResultDto
            {
                AccessToken = accessToken,
                ExpiresIn = _tokenSettings.AccessTokenExpirationMinutes * 60
            }, AuthOperationResult.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError("LinkedIn exchange failed: {Message}", ex.Message);

            return (null, AuthOperationResult.Error);
        }
    }
}

