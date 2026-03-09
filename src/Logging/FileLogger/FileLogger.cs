using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreMesh.Logging.FileLogger;

/// <summary>
/// An <see cref="ILogger"/> implementation that writes log entries to a file
/// via <see cref="LogFileProcesser"/>.
/// </summary>
internal class FileLogger(
    IConfiguration configuration,
    Func<string, bool> logLevelFunc,
    string categoryName,
    LogFileProcesser processer)
    : ILogger
{
    private readonly LogLevel _logLevel =
        configuration?.GetValue<LogLevel>("Logging:LogLevel:Default") ?? LogLevel.Information;

    /// <summary>
    /// Not supported. Always returns <see langword="null"/>.
    /// </summary>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return default;
    }

    /// <summary>
    /// Determines whether the given <paramref name="logLevel"/> is enabled
    /// based on the minimum level and category filter.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns><see langword="true"/> if the log level passes both the threshold and the category filter.</returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        if (logLevel < _logLevel)
            return false;

        return logLevelFunc(categoryName);
    }

    /// <summary>
    /// Formats and enqueues the log message for file writing.
    /// Does nothing if <see cref="IsEnabled"/> returns <see langword="false"/>.
    /// </summary>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        message = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{logLevel}] [{categoryName} - {message}]";

        processer.EnqueueMessage(message);
    }
}
