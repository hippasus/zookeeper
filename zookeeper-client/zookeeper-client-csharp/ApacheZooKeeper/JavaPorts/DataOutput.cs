namespace ApacheZooKeeper;

public class DataOutput
{
    /// <summary>
    /// Buffer used for temporary storage during conversion from primitives
    /// </summary>
    private readonly byte[] _buffer = new byte[16];

    private readonly Stream _stream;

    public DataOutput(Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanWrite)
        {
            throw new ArgumentException("Stream isn't writable", nameof(stream));
        }

        _stream = stream;
    }

    public void writeByte(byte value) => WriteInternal(new[] { value }, 1);

    /// <summary>
    /// Writes a boolean value to the stream. 1 byte is written.
    /// </summary>
    /// <param name="value">The value to write</param>
    public void writeBoolean(bool value)
    {
        BigEndianBitConverter.CopyBytes(value, _buffer, 0);
        WriteInternal(_buffer, 1);
    }

    /// <summary>
    /// Writes a 32-bit signed integer to the stream, using the bit converter
    /// for this writer. 4 bytes are written.
    /// </summary>
    /// <param name="value">The value to write</param>
    public void writeInt(int value)
    {
        BigEndianBitConverter.CopyBytes(value, _buffer, 0);
        WriteInternal(_buffer, 4);
    }

    /// <summary>
    /// Writes a 64-bit signed integer to the stream, using the bit converter
    /// for this writer. 8 bytes are written.
    /// </summary>
    /// <param name="value">The value to write</param>
    public void writeLong(long value)
    {
        BigEndianBitConverter.CopyBytes(value, _buffer, 0);
        WriteInternal(_buffer, 8);
    }

    /// <summary>
    /// Writes a float number to the stream, using the bit converter
    /// for this writer. 4 bytes are written.
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void writeFloat(float value)
    {
        //TODO: implement write float to stream
        throw new NotImplementedException();
    }

    /// <summary>
    /// Writes a float number to the stream, using the bit converter
    /// for this writer. 8 bytes are written.
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void writeDouble(double value)
    {
        //TODO: implement write double to stream
        throw new NotImplementedException();
    }

    /// <summary>
    /// Writes an array of bytes to the stream.
    /// </summary>
    /// <param name="value">The values to write</param>
    public void write(byte[] value)
    {
        if (value == null)
        {
            throw (new ArgumentNullException(nameof(value)));
        }

        WriteInternal(value, value.Length);
    }

    /// <summary>
    /// Writes a portion of bytes from the offset position to the stream.
    /// </summary>
    /// <param name="bytes">the original buffer</param>
    /// <param name="offset">offset position to start writing</param>
    /// <param name="count">bytes count</param>
    public void write(byte[] bytes, int offset, int count)
    {
        _stream.Write(bytes, offset, count);
    }

    private void WriteInternal(byte[] bytes, int length)
    {
        _stream.Write(bytes, 0, length);
    }
}
