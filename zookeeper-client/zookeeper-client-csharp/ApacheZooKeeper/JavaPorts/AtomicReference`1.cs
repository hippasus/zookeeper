namespace ApacheZooKeeper.JavaPorts;

public class AtomicReference<T>
    where T : class
{
    private volatile T _value;

    public AtomicReference(T initialValue)
    {
        _value = initialValue;
    }

    public T get()
    {
        return _value;
    }

    public void set(T newValue)
    {
        _value = newValue;
    }

    public bool compareAndSet(T expectedValue, T newValue)
    {
        return ReferenceEquals(Interlocked.CompareExchange(ref _value, newValue, expectedValue), expectedValue);
    }

    public T getAndSet(T newValue)
    {
        return Interlocked.Exchange(ref _value, newValue);
    }
}
