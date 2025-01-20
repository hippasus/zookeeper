namespace ApacheZooKeeper.JavaPorts;

public sealed class Iterator<T> : IDisposable
{
    private readonly IEnumerator<T> _enumerator;

    private bool? _hasNext = null; // Tracks if there are more elements

    public Iterator(IEnumerable<T> collection)
    {
        _enumerator = collection.GetEnumerator();
    }

    public bool hasNext()
    {
        _hasNext ??= _enumerator.MoveNext();

        return _hasNext.Value;
    }

    public T next()
    {
        if (_hasNext == null)
        {
            if (!hasNext())
            {
                throw new NoSuchElementException();
            }
        }

        var current = _enumerator.Current;
        _hasNext = null; // Reset has next state

        return current;
    }

    public void Dispose()
    {
        _enumerator?.Dispose();
    }
}

public class NoSuchElementException : Exception
{
}
