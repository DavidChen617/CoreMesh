using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreMesh.Logging.FileLogger;

/// <summary>
/// An <see cref="ILoggerProvider"/> that creates <see cref="FileLogger"/> instances.
/// </summary>
internal class FileLoggerProvider(
    IConfiguration configuration,
    Func<string, bool> logLevelFunc,
    LogFileProcesser processer) : ILoggerProvider
{
    /// <summary>
    /// Creates a <see cref="FileLogger"/> for the given <paramref name="categoryName"/>.
    /// </summary>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <returns>A new <see cref="FileLogger"/> instance.</returns>
    public ILogger CreateLogger(string categoryName)
        => new FileLogger(configuration, logLevelFunc, categoryName, processer);

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}
