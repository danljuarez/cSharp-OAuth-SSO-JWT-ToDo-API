using OauthSSOJwtTodoApiBackend.Enums;
using OauthSSOJwtTodoApiBackend.Models.DTOs.Auth;

namespace OauthSSOJwtTodoApiBackend.Services
{
    public interface IAuthService
    {
        Task<(JwtTokenResultDto? Data, AuthOperationResult Result)> ExchangeLinkedInCodeAsync(string code, string codeVerifier);
        Task<(LoginResultDto? Data, AuthOperationResult Result)> LoginAsync(LoginRequestDto request);
        Task<AuthOperationResult> LogoutAsync(string refreshToken);
        Task<(RefreshResultDto? Data, AuthOperationResult Result)> RefreshTokenAsync(string refreshToken);
    }
}