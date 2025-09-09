namespace OauthSSOJwtTodoApiBackend.Enums;

public enum AuthOperationResult
{
    Success,
    InvalidCredentials,
    InvalidInput,
    UserNotFound,
    TokenExpired,
    TokenInvalid,
    LinkedInExchangeFailed,
    LogoutInvalidToken,
    Error
}
