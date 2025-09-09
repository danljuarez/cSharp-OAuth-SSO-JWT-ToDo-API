using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OauthSSOJwtTodoApiBackend.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OauthSSOJwtTodoApiBackend.Middleware;

public class JwtCookieAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtCookieAuthMiddleware> _logger;

    public JwtCookieAuthMiddleware(
        RequestDelegate next,
        IOptions<JwtSettings> jwtOptions,
        ILogger<JwtCookieAuthMiddleware> logger)
    {
        _next = next;
        _jwtSettings = jwtOptions.Value;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // Skip this middleware if Authorization header is present
        if (context.Request.Headers.ContainsKey("Authorization"))
        {
            await _next(context);
            return;
        }

        var token = context.Request.Cookies["access_token"];

        if (!string.IsNullOrWhiteSpace(token))
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var validationParams = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = JwtRegisteredClaimNames.Sub // This maps "sub" to ClaimTypes.NameIdentifier
                };

                var principal = handler.ValidateToken(token, validationParams, out SecurityToken validatedToken);

                context.User = principal;

                _logger.LogDebug("JWT validated successfully from cookie. Access token will expire in {Minutes} minutes.",
                    _jwtSettings.AccessTokenExpirationMinutes);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "JWT validation failed from cookie: {Message}", ex.Message);

                // Clear invalid token
                context.Response.Cookies.Delete("access_token");

                // Optional: clear user principal explicitly
                context.User = new ClaimsPrincipal(new ClaimsIdentity());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during JWT validation from cookie.");
                context.Response.Cookies.Delete("access_token");
            }
        }

        await _next(context);
    }
}
