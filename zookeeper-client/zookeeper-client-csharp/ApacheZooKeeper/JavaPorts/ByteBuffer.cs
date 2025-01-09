namespace ApacheZooKeeper;

internal class ByteBuffer
{
    private byte[] _buffer;
    private int _position;
    private int _limit;
    private int _capacity;
    private int _mark;

    public ByteBuffer(int capacity)
    {
        _capacity = capacity;
        _buffer = new byte[capacity];
        _limit = capacity;
        _position = 0;
        _mark = -1;
    }

    public static ByteBuffer wrap(byte[] array)
    {
        ByteBuffer byteBuffer = new ByteBuffer(array.Length);
        Array.Copy(array, byteBuffer._buffer, array.Length);
        byteBuffer._limit = array.Length;
        return byteBuffer;
    }

    public static ByteBuffer allocate(int capacity) => new ByteBuffer(capacity);

    public ByteBuffer clear()
    {
        _limit = _capacity;
        _position = 0;
        _mark = -1;

        return this;
    }

    public int remaining() => _limit - _position;

    public ByteBuffer put(byte value)
    {
        if (_position >= _limit) throw new InvalidOperationException("Buffer overflow");
        _buffer[_position] = value;
        _position++;

        return this;
    }

    public ByteBuffer put(ByteBuffer src)
    {
        if (src == this)
            throw new InvalidOperationException("can not put itself");

        checkForOverflow(src.remaining());

        if (src.remaining() > 0)
        {
            byte[] toPut = new byte [src.remaining()];
            src.get(toPut);
            put(toPut);
        }

        return this;
    }

    public ByteBuffer put(byte[] src, int offset, int length)
    {
        checkArraySize(src.Length, offset, length);
        checkForOverflow(length);

        for (int i = offset; i < offset + length; i++)
            put(src[i]);

        return this;
    }

    public ByteBuffer put(byte[] src)
    {
        return put(src, 0, src.Length);
    }

    public byte get()
    {
        if (_position >= _limit) throw new InvalidOperationException("Buffer underflow");

        var r = _buffer[_position];
        _position++;

        return r;
    }

    public ByteBuffer get(byte[] dst, int offset, int length)
    {
        checkArraySize(dst.Length, offset, length);
        checkForUnderflow(length);

        for (int i = offset; i < offset + length; i++)
        {
            dst[i] = get();
        }

        return this;
    }

    public ByteBuffer get(byte[] dst)
    {
        return get(dst, 0, dst.Length);
    }

    public void putInt(int value)
    {
        put((byte)((value >> 24) & 0xFF));
        put((byte)((value >> 16) & 0xFF));
        put((byte)((value >> 8) & 0xFF));
        put((byte)(value & 0xFF));
    }

    public int getInt()
    {
        int value = 0;
        value |= (get() << 24);
        value |= (get() << 16);
        value |= (get() << 8);
        value |= get();
        return value;
    }

    public void flip()
    {
        _limit = _position;
        _position = 0;
        _mark = -1;
    }

    public void rewind()
    {
        _position = 0;
        _mark = -1;
    }

    public int capacity() => _capacity;

    public int position() => _position;

    public int limit() => _limit;

    public byte[] array() => _buffer;

    public bool hasRemaining() => remaining() > 0;

    private void checkForUnderflow()
    {
        if (!hasRemaining())
            throw new InvalidOperationException("Buffer underflow");
    }

    private void checkForUnderflow(int length)
    {
        if (remaining() < length)
            throw new InvalidOperationException("Buffer underflow");
    }

    private void checkForOverflow()
    {
        if (!hasRemaining())
            throw new InvalidOperationException("Buffer overflow");
    }

    void checkForOverflow(int length)
    {
        if (remaining() < length)
            throw new InvalidOperationException("Buffer overflow");
    }

    static void checkArraySize(int arraylength, int offset, int length)
    {
        if ((offset < 0) ||
            (length < 0) ||
            (arraylength < length + offset))
            throw new IndexOutOfRangeException();
    }
}
