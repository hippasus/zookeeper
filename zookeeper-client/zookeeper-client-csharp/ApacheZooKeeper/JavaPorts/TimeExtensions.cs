namespace ApacheZooKeeper.JavaPorts;

using System.Diagnostics;

public static class TimeExtensions
{
    public static long nanoTime() => Stopwatch.GetTimestamp() * 100L;

    public static long currentElapsedTime() => Stopwatch.GetTimestamp() * 100L;
}
