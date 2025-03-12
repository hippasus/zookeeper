namespace ApacheZooKeeper.Common;

internal class VolatileBoolean
{
    public VolatileBoolean(bool value)
    {
        Value = value;
    }

    private bool _value;

    public bool Value
    {
        get => Volatile.Read(ref _value);
        set => Volatile.Write(ref _value, value);
    }
}
