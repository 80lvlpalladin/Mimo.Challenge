using Microsoft.Extensions.Logging;

namespace Mimo.Challenge.Domain;

public partial class GlobalLogger
{
    /// <summary>
    /// Logs a handled exception.
    /// </summary>
    [LoggerMessage(1, LogLevel.Error, "{Message}, CorrelationId: {CorrelationId}", EventName = "HandledException")]
    public static partial void HandledException(ILogger logger, string message, string correlationId, Exception exception);
    
    /// <summary>
    /// Logs a warning.
    /// </summary>
    [LoggerMessage(2, LogLevel.Warning, "{Message}; UserId: {UserId}", EventName = "Warning")]
    public static partial void LogWarningForUser(ILogger logger, string message, uint userId);
}