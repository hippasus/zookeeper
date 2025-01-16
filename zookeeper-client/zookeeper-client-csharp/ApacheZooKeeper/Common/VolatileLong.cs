namespace ApacheZooKeeper.Common;

internal class VolatileLong
{
    public VolatileLong(long value)
    {
        Value = value;
    }

    private long _value;

    public long Value
    {
        get => Volatile.Read(ref _value);
        set => Volatile.Write(ref _value, value);
    }
}
