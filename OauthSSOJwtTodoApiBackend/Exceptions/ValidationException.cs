namespace OauthSSOJwtTodoApiBackend.Exceptions;

public class ValidationException : AppException
{
    public ValidationException(string message = "Validation failed.")
        : base(message, 400) { }
}
