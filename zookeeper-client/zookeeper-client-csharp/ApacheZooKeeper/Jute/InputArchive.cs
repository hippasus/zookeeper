namespace ApacheZooKeeper.Jute;

public interface InputArchive {

    byte readByte(String tag);

    bool readBool(String tag);

    int readInt(String tag);

    long readLong(String tag);

    float readFloat(String tag);

    double readDouble(String tag);

    string readString(String tag);

    byte[] readBuffer(String tag);

    void readRecord(Record r, String tag);

    void startRecord(String tag);

    void endRecord(String tag);

    Index startVector(String tag);

    void endVector(String tag);

    Index startMap(String tag);

    void endMap(String tag);

}
