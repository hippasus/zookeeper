namespace ApacheZooKeeper.Jute;

public interface OutputArchive {

    void writeByte(byte b, String tag);

    void writeBool(bool b, String tag);

    void writeInt(int i, String tag);

    void writeLong(long l, String tag);

    void writeFloat(float f, String tag);

    void writeDouble(double d, String tag);

    void writeString(String s, String tag);

    void writeBuffer(byte[] buf, String tag);

    void writeRecord(Record r, String tag);

    void startRecord(Record r, String tag);

    void endRecord(Record r, String tag);

    void startVector<T>(List<T> v, String tag);

    void endVector<T>(List<T> v, String tag);

    void startMap<TKey, TValue>(IDictionary<TKey, TValue> v, String tag);

    void endMap<TKey, TValue>(IDictionary<TKey, TValue> v, String tag);

    long getDataSize();

}
