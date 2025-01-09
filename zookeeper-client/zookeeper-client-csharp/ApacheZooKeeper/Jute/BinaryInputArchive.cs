namespace ApacheZooKeeper.Jute;

using System.Text;

public class BinaryInputArchive : InputArchive
{
    public static readonly String UNREASONBLE_LENGTH = "Unreasonable length = ";

    // CHECKSTYLE.OFF: ConstantName - for backward compatibility
    public static readonly int maxBuffer = 0xfffff;

    // CHECKSTYLE.ON:
    private static readonly int extraMaxBuffer = 1024;

    /*
    static {
        final Integer configuredExtraMaxBuffer =
            Integer.getInteger("zookeeper.jute.maxbuffer.extrasize", maxBuffer);
        if (configuredExtraMaxBuffer < 1024) {
            // Earlier hard coded value was 1024, So the value should not be less than that value
            extraMaxBuffer = 1024;
        } else {
            extraMaxBuffer = configuredExtraMaxBuffer;
        }
    }
    */

    private readonly DataInput _in;
    private readonly int totalBufferSize;

    /*
    public static BinaryInputArchive getArchive(InputStream stream)
    {
        return new BinaryInputArchive(new DataInputStream(stream));
    }
    */

    private class BinaryIndex : Index
    {
        private int n;

        public BinaryIndex(int nelems)
        {
            this.n = nelems;
        }

        public bool done()
        {
            return (n <= 0);
        }

        public void incr()
        {
            n--;
        }
    }

    /**
     * Creates a new instance of BinaryInputArchive.
     */
    public BinaryInputArchive(DataInput @in)
        : this(@in, maxBuffer, extraMaxBuffer)
    {
    }

    public BinaryInputArchive(DataInput @in, int maxBufferSize, int extraMaxBufferSize)
    {
        this._in = @in;
        if ((long)maxBufferSize + extraMaxBufferSize > int.MaxValue)
        {
            this.totalBufferSize = int.MaxValue;
        }
        else
        {
            this.totalBufferSize = maxBufferSize + extraMaxBufferSize;
        }
    }

    public byte readByte(String tag)
    {
        return _in.readByte();
    }

    public bool readBool(String tag)
    {
        return _in.readBoolean();
    }

    public int readInt(String tag)
    {
        return _in.readInt();
    }

    public long readLong(String tag)
    {
        return _in.readLong();
    }

    public float readFloat(String tag)
    {
        return _in.readFloat();
    }

    public double readDouble(String tag)
    {
        return _in.readDouble();
    }

    public string? readString(String tag)
    {
        int len = _in.readInt();
        if (len == -1)
        {
            return null;
        }

        checkLength(len);
        byte[] b = new byte[len];
        _in.readFully(b);
        //return new String(b, StandardCharsets.UTF_8);
        return Encoding.UTF8.GetString(b);
    }

    public byte[]? readBuffer(String tag)
    {
        int len = readInt(tag);
        if (len == -1)
        {
            return null;
        }

        checkLength(len);
        byte[] arr = new byte[len];
        _in.readFully(arr);
        return arr;
    }

    public void readRecord(Record r, String tag)
    {
        r.deserialize(this, tag);
    }

    public void startRecord(String tag)
    {
    }

    public void endRecord(String tag)
    {
    }

    public Index? startVector(String tag)
    {
        int len = readInt(tag);
        if (len == -1)
        {
            return null;
        }

        return new BinaryIndex(len);
    }

    public void endVector(String tag)
    {
    }

    public Index startMap(String tag)
    {
        return new BinaryIndex(readInt(tag));
    }

    public void endMap(String tag)
    {
    }

    // Since this is a rough sanity check, add some padding to maxBuffer to
    // make up for extra fields, etc. (otherwise e.g. clients may be able to
    // write buffers larger than we can read from disk!)
    private void checkLength(int len)
    {
        if (len < 0 || len > totalBufferSize)
        {
            throw new IOException(UNREASONBLE_LENGTH + len);
        }
    }
}
