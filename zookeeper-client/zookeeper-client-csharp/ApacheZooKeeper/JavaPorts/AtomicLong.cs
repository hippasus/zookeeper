namespace ApacheZooKeeper.JavaPorts;

public class AtomicLong
{
    private long _value;

    public AtomicLong(long value)
    {
        _value = value;
    }

    public bool compareAndSet(long expect, long update) => Interlocked.CompareExchange(ref _value, update, expect) == expect;

    public long get() => _value;

    public long getAndDecrement() => Interlocked.Decrement(ref _value);
    public long getAndIncrement() => Interlocked.Increment(ref _value);
}
