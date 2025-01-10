namespace ApacheZooKeeper.Logging;

public interface ILoggerFactory
{
    ILogger? CreateLogger(Type type);
}
