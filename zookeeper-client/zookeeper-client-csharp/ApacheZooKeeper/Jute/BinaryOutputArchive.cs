namespace ApacheZooKeeper.Jute;

using System.Text;

public class BinaryOutputArchive : OutputArchive {
    //private ByteBuffer bb = ByteBuffer.allocate(1024);

    private DataOutput _out;

    private long dataSize;

    /*
    public static BinaryOutputArchive getArchive(OutputStream strm) {
        return new BinaryOutputArchive(new DataOutputStream(strm));
    }
    */

    /**
     * Creates a new instance of BinaryOutputArchive.
     */
    public BinaryOutputArchive(DataOutput @out) {
        _out = @out;
    }

    public void writeByte(byte b, String tag) {
        _out.writeByte(b);
        dataSize += 1;
    }

    public void writeBool(bool b, String tag) {
        _out.writeBoolean(b);
        dataSize += 1;
    }

    public void writeInt(int i, String tag) {
        _out.writeInt(i);
        dataSize += 4;
    }

    public void writeLong(long l, String tag) {
        _out.writeLong(l);
        dataSize += 8;
    }

    public void writeFloat(float f, String tag) {
        _out.writeFloat(f);
        dataSize += 4;
    }

    public void writeDouble(double d, String tag) {
        _out.writeDouble(d);
        dataSize += 8;
    }

    /**
     * create our own char encoder to utf8. This is faster
     * then string.getbytes(UTF8).
     *
     * @param s the string to encode into utf8
     * @return utf8 byte sequence.
     */
    /*
    private ByteBuffer stringToByteBuffer(string s) {
        bb.clear();
        int len = s.Length;
        for (int i = 0; i < len; i++) {
            if (bb.remaining() < 3) {
                ByteBuffer n = ByteBuffer.allocate(bb.capacity() << 1);
                bb.flip();
                n.put(bb);
                bb = n;
            }
            char c = s[i];
            if (c < 0x80) {
                bb.put((byte) c);
            } else if (c < 0x800) {
                bb.put((byte) (0xc0 | (c >> 6)));
                bb.put((byte) (0x80 | (c & 0x3f)));
            } else {
                bb.put((byte) (0xe0 | (c >> 12)));
                bb.put((byte) (0x80 | ((c >> 6) & 0x3f)));
                bb.put((byte) (0x80 | (c & 0x3f)));
            }
        }
        bb.flip();
        return bb;
    }
    */

    public void writeString(String s, String tag) {
        if (s == null) {
            writeInt(-1, "len");
            return;
        }
        //ByteBuffer bb = stringToByteBuffer(s);
        //int strLen = bb.remaining();
        var bb = Encoding.UTF8.GetBytes(s);
        var strLen = bb.Length;
        writeInt(strLen, "len");
        _out.write(bb);
        //_out.write(bb.array(), bb.position(), bb.limit());
        dataSize += strLen;
    }

    public void writeBuffer(byte[] barr, String tag)
            {
        if (barr == null) {
            writeInt(-1, "len");
            return;
        }
        int len = barr.Length;
        writeInt(len, "len");
        _out.write(barr);
        dataSize += len;
    }

    public void writeRecord(Record r, String tag) {
        r.serialize(this, tag);
    }

    public void startRecord(Record r, String tag) {
    }

    public void endRecord(Record r, String tag) {
    }

    public void startVector<T>(List<T> v, String tag) {
        if (v == null) {
            writeInt(-1, tag);
            return;
        }
        writeInt(v.Count, tag);
    }

    public void endVector<T>(List<T> v, String tag) {
    }

    public void startMap<TKey, TValue>(IDictionary<TKey, TValue> v, String tag) {
        writeInt(v.Count, tag);
    }

    public void endMap<TKey, TValue>(IDictionary<TKey, TValue> v, String tag) {
    }

    public long getDataSize() {
        return dataSize;
    }
}
