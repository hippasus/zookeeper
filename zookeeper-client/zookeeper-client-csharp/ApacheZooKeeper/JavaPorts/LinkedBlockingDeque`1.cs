namespace ApacheZooKeeper.JavaPorts;

using System.Collections;

public class LinkedBlockingDeque<T> : IEnumerable<T>
{
    private readonly Node _head;
    private readonly Node _tail;
    private readonly int _capacity;
    private int _count;
    private readonly object _lockObject = new object();
    private readonly ManualResetEventSlim _notEmptyEvent = new ManualResetEventSlim(false);

    public LinkedBlockingDeque()
        : this(int.MaxValue)
    {
    }

    public LinkedBlockingDeque(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));
        }

        this._capacity = capacity;
        _head = new(default);
        _tail = new(default);
        _head.Next = _tail;
        _tail.Prev = _head;
    }

    private bool isEmpty() => _count == 0;

    public void addFirst(T item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        lock (_lockObject)
        {
            while (_count == _capacity)
            {
                Monitor.Wait(_lockObject);
            }

            linkFirst(item);

            Monitor.PulseAll(_lockObject);
        }
    }

    public bool tryAddFirst(T item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        lock (_lockObject)
        {
            if (_count == _capacity)
            {
                return false;
            }

            linkFirst(item);

            return true;
        }
    }

    public void add(T item) => addLast(item);

    public void addLast(T item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        lock (_lockObject)
        {
            while (_count == _capacity)
            {
                Monitor.Wait(_lockObject);
            }

            linkLast(item);

            Monitor.PulseAll(_lockObject);
        }
    }

    public bool tryAddLast(T item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        lock (_lockObject)
        {
            if (_count == _capacity)
            {
                return false;
            }

            linkLast(item);

            return true;
        }
    }

    public T poll() => pollFirst();

    public T pollFirst()
    {
        lock (_lockObject)
        {
            var firstNode = unlinkFirst();
            var item = firstNode == null ? default : firstNode.Item;
            return item;
        }
    }

    public bool tryPollFirst(out T item)
    {
        item = default;

        lock (_lockObject)
        {
            var firstNode = unlinkFirst();
            if (firstNode == null)
            {
                return false;
            }

            item = firstNode.Item;

            return true;
        }
    }

    public T takeFirst()
    {
        T item = default;
        var empty = false;

        do
        {
            if (empty)
            {
                _notEmptyEvent.Wait();
            }

            lock (_lockObject)
            {
                empty = isEmpty();

                if (!empty)
                {
                    item = unlinkFirst().Item;
                }
            }
        } while (empty);

        return item;
    }

    public T pollLast()
    {
        lock (_lockObject)
        {
            var lastNode = unlinkLast();
            var item = lastNode == null ? default : lastNode.Item;
            return item;
        }
    }

    public bool tryPollLast(out T item)
    {
        item = default;
        lock (_lockObject)
        {
            var lastNode = unlinkLast();
            if (lastNode == null)
            {
                return false;
            }

            item = lastNode.Item;

            return true;
        }
    }

    public T takeLast()
    {
        T item = default;
        var empty = false;

        do
        {
            if (empty)
            {
                _notEmptyEvent.Wait();
            }

            lock (_lockObject)
            {
                empty = isEmpty();

                if (!empty)
                {
                    item = unlinkLast().Item;
                }
            }
        } while (empty);

        return item;
    }

    private Node linkFirst(T item)
    {
        var newFirstNode = new Node(item);
        var oldFirstNode = _head.Next;
        newFirstNode.Next = oldFirstNode;
        oldFirstNode.Prev = newFirstNode;
        _head.Next = newFirstNode;
        newFirstNode.Prev = _head;
        _count++;

        _notEmptyEvent.Set();

        return newFirstNode;
    }

    private void linkLast(T item)
    {
        var newLastNode = new Node(item);
        var oldLastNode = _tail.Prev;
        newLastNode.Prev = oldLastNode;
        oldLastNode.Next = newLastNode;
        newLastNode.Next = _tail;
        _tail.Prev = newLastNode;
        _count++;

        _notEmptyEvent.Set();
    }

    private Node unlinkFirst()
    {
        var oldFirstNode = _head.Next;
        if (oldFirstNode == _tail)
        {
            return null;
        }

        var newFirstNode = oldFirstNode.Next;
        newFirstNode.Prev = _head;
        _head.Next = newFirstNode;
        _count--;

        if (isEmpty())
        {
            _notEmptyEvent.Reset();
        }

        return oldFirstNode;
    }

    private Node unlinkLast()
    {
        var oldLastNode = _tail.Prev;
        if (oldLastNode == _head)
        {
            return null;
        }

        var newLastNode = oldLastNode.Prev;
        newLastNode.Next = _tail;
        _tail.Prev = newLastNode;
        _count--;

        if (isEmpty())
        {
            _notEmptyEvent.Reset();
        }

        return oldLastNode;
    }

    public int size() => _count;

    private class Node
    {
        public T Item { get; set; }
        public Node Prev { get; set; }
        public Node Next { get; set; }

        public Node(T item)
        {
            Item = item;
        }
    }

    private class LinkedBlockingDequeEnumerator : IEnumerator<T>
    {
        private readonly LinkedBlockingDeque<T> _instance;
        private Node _currentNode;

        public LinkedBlockingDequeEnumerator(LinkedBlockingDeque<T> instance)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _currentNode = instance._head;
        }

        public bool MoveNext()
        {
            var nextNode = _currentNode.Next;
            if (nextNode != null && nextNode != _instance._tail)
            {
                return false;
            }

            _currentNode = nextNode;

            return true;
        }

        public void Reset()
        {
            _currentNode = _instance._head;
        }

        public T Current => _currentNode.Item;
        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }

    public Iterator<T> iterator() => new(this);

    public IEnumerator<T> GetEnumerator()
    {
        return new LinkedBlockingDequeEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
