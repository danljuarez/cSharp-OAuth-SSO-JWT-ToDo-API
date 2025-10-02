using Microsoft.Extensions.Logging;
using Moq;

namespace OauthSSOJwtTodoApiBackend.Tests.xUnit.TestUtils;

// Logger extension method for log testing
public static class LoggerExtensions
{
    public static void VerifyLog(this Mock<ILogger> logger, LogLevel level, string containsMessage, Times times)
    {
        logger.Verify(
            l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                     v != null && v.ToString()!.Contains(containsMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            times
        );
    }

    public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, string containsMessage, Times times)
    {
        logger.As<ILogger>().VerifyLog(level, containsMessage, times);
    }
}
