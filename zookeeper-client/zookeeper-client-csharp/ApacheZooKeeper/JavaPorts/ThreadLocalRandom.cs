namespace ApacheZooKeeper.JavaPorts;

public static class ThreadLocalRandom
{
    private static ThreadLocal<Random> _random = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

    public static Random current() => _random.Value;
}
