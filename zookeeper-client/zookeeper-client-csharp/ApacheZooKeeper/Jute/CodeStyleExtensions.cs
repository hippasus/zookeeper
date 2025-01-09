namespace ApacheZooKeeper.Jute;

public static class IndexCodeStyleExtensions
{
    public static bool Done(this Index o) => o.done();
    public static void Incr(this Index o) => o.incr();
}

public static class InputArchiveCodeStyleExtensions
{
    public static byte ReadByte(this InputArchive o, string tag) => o.readByte(tag);

    public static bool ReadBool(this InputArchive o, string tag) => o.readBool(tag);

    public static int ReadInt(this InputArchive o, string tag) => o.readInt(tag);

    public static long ReadLong(this InputArchive o, string tag) => o.readLong(tag);

    public static float ReadFloat(this InputArchive o, string tag) => o.readFloat(tag);

    public static double ReadDouble(this InputArchive o, string tag) => o.readDouble(tag);

    public static string? ReadString(this InputArchive o, string tag) => o.readString(tag);

    public static byte[]? ReadBuffer(this InputArchive o, string tag) => o.readBuffer(tag);

    public static void ReadRecord(this InputArchive o, Record r, string tag) => o.readRecord(r, tag);

    public static void StartRecord(this InputArchive o, string tag) => o.startRecord(tag);

    public static void EndRecord(this InputArchive o, string tag) => o.endRecord(tag);

    public static Index? StartVector(this InputArchive o, string tag) => o.startVector(tag);

    public static void EndVector(this InputArchive o, string tag) => o.endVector(tag);

    public static Index? StartMap(this InputArchive o, string tag) => o.startMap(tag);

    public static void EndMap(this InputArchive o, string tag) => o.endMap(tag);
}

public static class OutputArchiveCodeStyleExtensions
{
    public static void WriteByte(this OutputArchive o, byte b, string tag) => o.writeByte(b, tag);

    public static void WriteBool(this OutputArchive o, bool b, string tag) => o.writeBool(b, tag);

    public static void WriteInt(this OutputArchive o, int i, string tag) => o.writeInt(i, tag);

    public static void WriteLong(this OutputArchive o, long l, string tag) => o.writeLong(l, tag);

    public static void WriteFloat(this OutputArchive o, float f, string tag) => o.writeFloat(f, tag);

    public static void WriteDouble(this OutputArchive o, double d, string tag) => o.writeDouble(d, tag);

    public static void WriteString(this OutputArchive o, string s, string tag) => o.writeString(s, tag);

    public static void WriteBuffer(this OutputArchive o, byte[] buf, string tag) => o.writeBuffer(buf, tag);

    public static void WriteRecord(this OutputArchive o, Record r, string tag) => o.writeRecord(r, tag);

    public static void StartRecord(this OutputArchive o, Record r, string tag) => o.startRecord(r, tag);

    public static void EndRecord(this OutputArchive o, Record r, string tag) => o.endRecord(r, tag);

    public static void StartVector<T>(this OutputArchive o, List<T> v, string tag) => o.startVector(v, tag);

    public static void EndVector<T>(this OutputArchive o, List<T> v, string tag) => o.endVector(v, tag);

    public static void StartMap<TKey, TValue>(this OutputArchive o, IDictionary<TKey, TValue> v, string tag) => o.startMap(v, tag);

    public static void EndMap<TKey, TValue>(this OutputArchive o, IDictionary<TKey, TValue> v, string tag) => o.endMap(v, tag);

    public static long GetDataSize(this OutputArchive o) => o.getDataSize();
}

public static class RecordCodeStyleExtensions
{
    public static void serialize(this Record o, OutputArchive archive, string tag) => o.Serialize(archive, tag);
    public static void deserialize(this Record o, InputArchive archive, string tag) => o.Deserialize(archive, tag);
}
