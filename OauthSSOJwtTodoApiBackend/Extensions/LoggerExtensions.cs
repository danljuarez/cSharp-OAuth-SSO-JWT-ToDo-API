namespace OauthSSOJwtTodoApiBackend.Logging;

/// <summary>
/// Extension methods for ILogger to assist handling token truncation for secure and readable logging.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Truncates a token to a safer, readable format for logging.
    /// Example: abcdefghijklmnop → abcd...mnop
    /// </summary>
    /// <param name="logger">ILogger instance (extension target)</param>
    /// <param name="token">The sensitive token string</param>
    /// <returns>A truncated version of the token</returns>
    public static string TruncateToken(this ILogger logger, string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return "[null or empty token]";

        return token.Length <= 8
            ? token
            : $"{token[..4]}...{token[^4..]}";
    }

    /// <summary>
    /// Logs a token-related message using a truncated version of the token.
    /// </summary>
    /// <param name="logger">ILogger instance</param>
    /// <param name="message">Message template (e.g., "Logged out token: {Token}")</param>
    /// <param name="token">The full token to be truncated for logging</param>
    public static void LogTokenInfo(this ILogger logger, string message, string token)
    {
        logger.LogInformation(message, logger.TruncateToken(token));
    }
}
