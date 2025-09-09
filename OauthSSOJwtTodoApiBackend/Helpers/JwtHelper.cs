using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OauthSSOJwtTodoApiBackend.Configuration;
using OauthSSOJwtTodoApiBackend.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OauthSSOJwtTodoApiBackend.Helpers;

public class JwtHelper
{
    private readonly JwtSettings _jwtSettings;

    public JwtHelper(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),           // Subject (User ID)
            new Claim(ClaimTypes.Name, user.Username),                            // Friendly name
            new Claim(ClaimTypes.Email, user.Email),                              // Email
            new Claim(ClaimTypes.Role, user.Role),                                // Role - REQUIRED for [Authorize(Roles = "...")]
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())     // Unique token ID
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
