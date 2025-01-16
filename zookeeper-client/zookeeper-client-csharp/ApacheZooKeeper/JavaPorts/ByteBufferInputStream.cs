namespace ApacheZooKeeper.JavaPorts;

internal class ByteBufferInputStream : Stream
{
    private readonly ByteBuffer _bb;

    public ByteBufferInputStream(ByteBuffer bb)
    {
        _bb = bb;
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        _bb.get(buffer, offset, count);
        return count;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException("Seek a ByteBufferInputStream is not supported");
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("SetLength for a ByteBufferInputStream is not supported");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _bb.put(buffer, offset, count);
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => _bb.capacity();

    public override long Position
    {
        get => _bb.position();
        set => throw new NotSupportedException("Seek a ByteBufferInputStream is not supported");
    }
}
