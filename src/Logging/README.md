English | [繁體中文](README.zh-TW.md)

# CoreMesh.Logging

File logger provider for `Microsoft.Extensions.Logging` with automatic file rotation and category filtering.

## Usage

```csharp
// Default options
builder.Logging.AddFileLogger(builder.Configuration);

// Custom options
builder.Logging.AddFileLogger(builder.Configuration, options =>
{
    options.Path = "./Logs/app.log";
    options.MaxFileSize = 1024 * 1024 * 10; // 10 MB
    options.AddFilter(category => category.StartsWith("MyApp"));
});
```

## Options

| Property | Default | Description |
|----------|---------|-------------|
| `Path` | `./Logs/log.txt` | Log file path (directory is created automatically) |
| `MaxFileSize` | 2048 (2 KB) | File size limit in bytes before rotation |
| `FileBufferSize` | 2048 (2 KB) | File stream buffer size in bytes |

`AddFilter(predicate)` restricts which categories are written to file. Multiple filters are evaluated with AND logic.

The minimum log level is read from `Logging:LogLevel:Default` in configuration (defaults to `Information`).

## Log Format

```
[2026-01-15 08:30:00] [Information] [MyApp.Services.UserService - User 42 created]
```

## Notes

- Log entries are written asynchronously via a background queue (`LogFileProcesser`).
- On rotation, the existing file is renamed with a timestamp suffix and a new file is started.
- `BeginScope` is not supported.
