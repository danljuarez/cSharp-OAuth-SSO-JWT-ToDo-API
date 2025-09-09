namespace OauthSSOJwtTodoApiBackend.Models.DTOs.Common;

/// <summary>
/// DTO representing a standardized error response returned by the API.
/// </summary>
public class ErrorResponseDto
{
    public string Error { get; set; } = "Internal Server Error";
    public string Message { get; set; } = "An unexpected error occurred.";
    public int Status { get; set; }
}
