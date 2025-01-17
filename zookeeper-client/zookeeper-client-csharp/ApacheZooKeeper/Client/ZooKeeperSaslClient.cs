namespace ApacheZooKeeper.Client;

public class ZooKeeperSaslClient
{
    public enum SaslState
    {
        INITIAL,
        INTERMEDIATE,
        COMPLETE,
        FAILED,
    }

    public void respondToServer(byte[] serverToken, ClientCnxn cnxn)
    {
    }

    public string getConfigStatus() => string.Empty;

    public SaslState getSaslState()
    {
        throw new NotImplementedException();
    }

    public void initialize(ClientCnxn clientCnxn)
    {
        throw new NotImplementedException();
    }

    public Watcher.Event.KeeperState getKeeperState()
    {
        throw new NotImplementedException();
    }

    public bool clientTunneledAuthenticationInProgress()
    {
        throw new NotImplementedException();
    }
}
