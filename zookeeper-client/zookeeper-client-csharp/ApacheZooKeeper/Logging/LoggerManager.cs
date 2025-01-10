namespace ApacheZooKeeper.Logging;

public static class LoggerManager
{
    public static ILoggerFactory? LoggerFactory { get; set; }

    public static ILogger GetLogger<T>()
    {
        return GetLogger(typeof(T));
    }

    public static ILogger GetLogger(Type type) => LoggerFactory?.CreateLogger(type) ?? NoLogger.Instance;
}
