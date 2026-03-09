namespace CoreMesh.Logging.FileLogger;

/// <summary>
/// Configuration options for the file logger.
/// </summary>
public class FileLoggerOptions
{
    /// <summary>
    /// Gets or sets the maximum file size in bytes before rotation occurs. Defaults to 2 KB.
    /// </summary>
    public int MaxFileSize { get; set; } = 1024 * 2;

    /// <summary>
    /// Gets or sets the file stream buffer size in bytes. Defaults to 2 KB.
    /// </summary>
    public int FileBufferSize { get; set; } = 1024 * 2;

    /// <summary>
    /// Gets or sets the log file path. Defaults to <c>./Logs/log.txt</c>.
    /// </summary>
    public string Path { get; set; } = DefaultPath;

    private static readonly string DefaultPath = Directory.GetCurrentDirectory() + "/Logs/log.txt";
    private readonly List<Func<string, bool>> _filters = new();

    /// <summary>
    /// Adds a category filter. Only categories that pass all filters will be logged.
    /// </summary>
    /// <param name="filter">A predicate that receives the category name and returns <see langword="true"/> to allow logging.</param>
    /// <returns>The current <see cref="FileLoggerOptions"/> instance.</returns>
    public FileLoggerOptions AddFilter(Func<string, bool> filter)
    {
        _filters.Add(filter);
        return this;
    }

    /// <summary>
    /// Evaluates whether the given <paramref name="category"/> passes all registered filters.
    /// </summary>
    /// <param name="category">The logger category name.</param>
    /// <returns><see langword="true"/> if no filters are registered or all filters pass.</returns>
    public bool PassFilter(string category) =>
        _filters.Count == 0 || _filters.All(f => f(category));
}
