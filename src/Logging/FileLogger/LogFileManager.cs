using System.Text;
using Microsoft.Extensions.Options;

namespace CoreMesh.Logging.FileLogger;

/// <summary>
/// Manages the log file stream, including writing and rotating log files when the size limit is exceeded.
/// </summary>
internal class LogFileManager : IDisposable, IAsyncDisposable
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private string _currentFilePath;
    private long _currentFileSize;
    private readonly string _logPath;
    private readonly int _maxFileSize;
    private readonly int _fileBufferSize;
    private FileStream _fileStream;

    /// <summary>
    /// Initializes a new instance of <see cref="LogFileManager"/>,
    /// creating the log directory if it does not exist.
    /// </summary>
    /// <param name="options">The file logger options.</param>
    public LogFileManager(IOptions<FileLoggerOptions> options)
    {
        _logPath = options.Value.Path;
        _maxFileSize = options.Value.MaxFileSize;
        _fileBufferSize = options.Value.FileBufferSize;
        var directory = Path.GetDirectoryName(_logPath);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        _currentFilePath = _logPath;

        _currentFileSize = File.Exists(_currentFilePath)
            ? new FileInfo(_currentFilePath).Length
            : 0;

        _fileStream = new FileStream(_currentFilePath, FileMode.Append, FileAccess.Write,
            FileShare.Read, _fileBufferSize, useAsync: true);
    }

    /// <summary>
    /// Writes the given <paramref name="message"/> to the current log file.
    /// Rotates to a new file if the size limit is exceeded.
    /// </summary>
    /// <param name="message">The log message to write.</param>
    internal async Task WriteLogToFileAsync(string message)
    {
        try
        {
            await _semaphoreSlim.WaitAsync();

            if (_currentFileSize > _maxFileSize)
            {
                await _fileStream.DisposeAsync();
                _currentFilePath = GetNextFilePath();
                _currentFileSize = 0;
                _fileStream = new FileStream(_currentFilePath, FileMode.Append, FileAccess.Write,
                    FileShare.Read, _fileBufferSize, useAsync: true);
            }

            var encodedText = Encoding.UTF8.GetBytes(message + Environment.NewLine);

            await _fileStream.WriteAsync(encodedText);

            _currentFileSize += encodedText.Length;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private string GetNextFilePath()
    {
        var baseFileName = Path.GetFileNameWithoutExtension(_logPath);
        var directory = Path.GetDirectoryName(_logPath);
        var extension = Path.GetExtension(_logPath);

        for (int i = 0;; i++)
        {
            var filePath = Path.Combine(directory!, $"{baseFileName}_{i}{extension}");
            if (!File.Exists(filePath))
                return filePath;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _semaphoreSlim.Dispose();
        _fileStream.Dispose();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        _semaphoreSlim.Dispose();
        await _fileStream.DisposeAsync();
    }
}
