namespace ApacheZooKeeper.JavaPorts;

using System.Diagnostics;

public static class Time
{
    public static long nanoTime() => Stopwatch.GetTimestamp() * (1_000_000_000L / Stopwatch.Frequency);

    public static long currentElapsedTime() => nanoTime() / 1_000_000L;
}
