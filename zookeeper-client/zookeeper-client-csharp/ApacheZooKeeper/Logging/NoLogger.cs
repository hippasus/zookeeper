namespace ApacheZooKeeper.Logging;

class NoLogger : ILogger
{
    internal static readonly NoLogger Instance = new();

    private NoLogger()
    {
    }

    public void Log(LogLevel logLevel, Exception exception, string message, params object[] args)
    {
    }

    public bool IsEnabled(LogLevel logLevel) => false;
}
