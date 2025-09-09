using Microsoft.AspNetCore.Mvc;
using OauthSSOJwtTodoApiBackend.Enums;
using OauthSSOJwtTodoApiBackend.Models.DTOs.Auth;
using OauthSSOJwtTodoApiBackend.Services;

namespace OauthSSOJwtTodoApiBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var (data, result) = await _auth.LoginAsync(dto);
        return MapAuthResult(result, data);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        var (data, result) = await _auth.RefreshTokenAsync(dto.RefreshToken);
        return MapAuthResult(result, data);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _auth.LogoutAsync(dto.RefreshToken);
        return result switch
        {
            AuthOperationResult.Success => Ok("Logged out"),
            AuthOperationResult.LogoutInvalidToken => Unauthorized("Invalid refresh token."),
            _ => StatusCode(500, "Unexpected error")
        };
    }

    [HttpPost("exchange-linkedin")]
    public async Task<IActionResult> ExchangeCode([FromBody] LinkedInExchangeDto dto)
    {
        var (data, result) = await _auth.ExchangeLinkedInCodeAsync(dto.Code, dto.CodeVerifier);
        return MapAuthResult(result, data);
    }

    // Helper for mapping AuthOperationResult
    private IActionResult MapAuthResult<T>(AuthOperationResult result, T? value)
    {
        return result switch
        {
            AuthOperationResult.Success => Ok(value),
            AuthOperationResult.InvalidCredentials => Unauthorized("Invalid credentials."),
            AuthOperationResult.UserNotFound => NotFound("User not found."),
            AuthOperationResult.TokenInvalid => Unauthorized("Invalid or expired token."),
            AuthOperationResult.InvalidInput => BadRequest("Invalid input."),
            _ => StatusCode(500, "Unexpected error.")
        };
    }
}
