namespace OauthSSOJwtTodoApiBackend.Services;

/// <summary>
/// Custom logger service that wraps the built-in ILogger interface.
///
/// - This class provides centralized logging methods for Info, Warn, and Error levels.
/// - Note: This service is currently not in use. The application uses ILogger<T> directly in services.
///
/// Intended to be used when integrating external logging sinks such as Sentry, Seq, or others.
/// </summary>
public class LoggerService : ILoggerService
{
    private readonly ILogger<LoggerService> _logger;

    /// <summary>
    /// Constructor that injects the built-in logger.
    /// </summary>
    public LoggerService(ILogger<LoggerService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    public void Info(string message, params object[]? args) =>
        _logger.LogInformation(message, args ?? Array.Empty<object>());

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public void Warn(string message, params object[]? args) =>
        _logger.LogWarning(message, args ?? Array.Empty<object>());

    /// <summary>
    /// Logs an error message, with optional exception.
    /// </summary>
    public void Error(string message, Exception? ex = null, params object[]? args) =>
        _logger.LogError(ex, message, args ?? Array.Empty<object>());
}
