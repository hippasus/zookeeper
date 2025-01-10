namespace ApacheZooKeeper;

public class DataInput
{
    private readonly Stream _stream;

    /// <summary>
    /// Buffer used for temporary storage before conversion into primitives
    /// </summary>
    private readonly byte[] _byteBuffer = new byte[16];

    public DataInput(Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanRead)
        {
            throw new ArgumentException("Stream isn't readable", nameof(stream));
        }

        _stream = stream;
    }

    /// <summary>
    /// Reads byte from the stream, 1 byte is read
    /// </summary>
    /// <returns></returns>
    public byte readByte()
    {
        ReadExactSizeOfBytes(_byteBuffer, 1);
        return _byteBuffer[0];
    }

    /// <summary>
    /// Reads a boolean from the stream. 1 byte is read.
    /// </summary>
    /// <returns>The boolean read</returns>
    public bool readBoolean()
    {
        ReadExactSizeOfBytes(_byteBuffer, 1);
        return BigEndianBitConverter.ToBoolean(_byteBuffer, 0);
    }

    /// <summary>
    /// Reads a 32-bit signed integer from the stream, using the bit converter
    /// for this reader. 4 bytes are read.
    /// </summary>
    /// <returns>The 32-bit integer read</returns>
    public int readInt()
    {
        ReadExactSizeOfBytes(_byteBuffer, 4);
        return BigEndianBitConverter.ToInt32(_byteBuffer, 0);
    }

    /// <summary>
    /// Reads a 64-bit signed integer from the stream, using the bit converter
    /// for this reader. 8 bytes are read.
    /// </summary>
    /// <returns>The 64-bit integer read</returns>
    public long readLong()
    {
        ReadExactSizeOfBytes(_byteBuffer, 8);
        return BigEndianBitConverter.ToInt64(_byteBuffer, 0);
    }

    /// <summary>
    /// Reads a single-precision floating-point value from the stream, using the bit converter
    /// for this reader. 4 bytes are read.
    /// </summary>
    /// <returns>The floating point value read</returns>
    public float readFloat()
    {
        ReadExactSizeOfBytes(_byteBuffer, 4);
        return BigEndianBitConverter.ToSingle(_byteBuffer, 0);
    }

    /// <summary>
    /// Reads a double-precision floating-point value from the stream, using the bit converter
    /// for this reader. 8 bytes are read.
    /// </summary>
    /// <returns>The double floating point value read</returns>
    public double readDouble()
    {
        ReadExactSizeOfBytes(_byteBuffer, 8);
        return BigEndianBitConverter.ToDouble(_byteBuffer, 0);
    }

    /// <summary>
    /// Reads the specified number of bytes, returning them in a new byte array.
    /// If not enough bytes are available before the end of the stream, this
    /// method will throw an IOException.
    /// </summary>
    /// <param name="count">The number of bytes to read</param>
    /// <returns>The bytes read</returns>
    public byte[] readBytesOrThrow(int count)
    {
        byte[] ret = new byte[count];
        ReadExactSizeOfBytes(ret, count);
        return ret;
    }

    /// <summary>
    /// Reads and fill the byte buffer fully
    /// If not enough bytes are available before the end of the stream, this
    /// method will throw an IOException.
    /// </summary>
    /// <param name="buffer">the target byte buffer</param>
    public void readFully(byte[] buffer)
    {
        ReadExactSizeOfBytes(buffer, buffer.Length);
    }

    private void ReadExactSizeOfBytes(byte[] data, int size)
    {
        int index = 0;
        while (index < size)
        {
            int read = _stream.Read(data, index, size - index);
            if (read == 0)
            {
                throw new EndOfStreamException("End of stream reached with " + (size - index) + "byte(s) left to read.");
            }

            index += read;
        }
    }
}
