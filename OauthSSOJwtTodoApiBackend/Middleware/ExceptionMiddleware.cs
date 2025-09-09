using Newtonsoft.Json;
using OauthSSOJwtTodoApiBackend.Exceptions;
using OauthSSOJwtTodoApiBackend.Models.DTOs.Common;

namespace OauthSSOJwtTodoApiBackend.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);  // Continue the pipeline
        }
        catch (AppException ex)
        {
            await HandleExceptionAsync(context, ex, ex.StatusCode);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode)
    {
        _logger.LogError(exception, "An exception occurred.");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var errorResponse = new ErrorResponseDto
        {
            Status = statusCode,
            Error = GetErrorTitle(statusCode),
            Message = _env.IsDevelopment()
                ? exception.Message // Show full message in dev
                : "An unexpected error occurred. Please try again later." // Hide internals in prod
        };

        var result = JsonConvert.SerializeObject(errorResponse);
        await context.Response.WriteAsync(result);
    }

    private string GetErrorTitle(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            500 => "Internal Server Error",
            _ => "Error"
        };
    }
}
