namespace ApacheZooKeeper.Jute;

public interface Record {
    void Serialize(OutputArchive archive, String tag);
    void Deserialize(InputArchive archive, String tag);
}
