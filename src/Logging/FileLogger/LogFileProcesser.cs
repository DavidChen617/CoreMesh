using System.Threading.Channels;

namespace CoreMesh.Logging.FileLogger;

/// <summary>
/// Processes log messages asynchronously via a bounded channel,
/// writing each message to the file through <see cref="LogFileManager"/>.
/// </summary>
internal class LogFileProcesser : IDisposable, IAsyncDisposable
{
    private readonly Channel<string> _logChannel = Channel.CreateBounded<string>(
        new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });

    private readonly Task _processTask;
    private readonly LogFileManager _logFileManager;

    /// <summary>
    /// Initializes a new instance of <see cref="LogFileProcesser"/> and starts the background processing loop.
    /// </summary>
    /// <param name="logFileManager">The file manager used for writing log entries.</param>
    public LogFileProcesser(LogFileManager logFileManager)
    {
        _logFileManager = logFileManager;
        _processTask = Task.Run(ProcessLogsAsync);
    }

    /// <summary>
    /// Enqueues a log message for asynchronous file writing.
    /// If the channel is full, the oldest message is dropped.
    /// </summary>
    /// <param name="message">The formatted log message to write.</param>
    public void EnqueueMessage(string message)
    {
        _logChannel.Writer.TryWrite(message);
    }

    private async Task ProcessLogsAsync()
    {
        await foreach (var message in _logChannel.Reader.ReadAllAsync())
        {
            try
            {
                await _logFileManager.WriteLogToFileAsync(message);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"[FileLogger] Write failed: " + ex.Message);
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _logChannel.Writer.Complete();
        _processTask.GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        _logChannel.Writer.Complete();
        await _processTask;
        await _logFileManager.DisposeAsync();
    }
}
