namespace ApacheZooKeeper.Logging;

public interface ILogger
{
    void Log(LogLevel logLevel, Exception? exception, string? message, params object?[] args);

    bool IsEnabled(LogLevel logLevel);
}
