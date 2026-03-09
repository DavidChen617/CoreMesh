using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoreMesh.Logging.FileLogger;

/// <summary>
/// Provides extension methods for registering the file logger.
/// </summary>
public static class FileLoggerExtension
{
    extension(ILoggingBuilder builder)
    {
        /// <summary>
        /// Adds the file logger with default options.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The logging builder.</returns>
        public ILoggingBuilder AddFileLogger(IConfiguration configuration) =>
            builder.AddFileLogger(configuration, _ => { });

        /// <summary>
        /// Adds the file logger with custom options.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="configureOptions">A delegate to configure <see cref="FileLoggerOptions"/>.</param>
        /// <returns>The logging builder.</returns>
        public ILoggingBuilder AddFileLogger(IConfiguration configuration, Action<FileLoggerOptions> configureOptions)
        {
            builder.Services
                .Configure(configureOptions)
                .AddSingleton<LogFileManager>()
                .AddSingleton<LogFileProcesser>()
                .AddSingleton<ILoggerProvider>(
                    sp => new FileLoggerProvider(
                        configuration,
                        sp.GetRequiredService<IOptions<FileLoggerOptions>>().Value.PassFilter,
                        sp.GetRequiredService<LogFileProcesser>()));

            return builder;
        }
    }
}
