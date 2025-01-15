namespace ApacheZooKeeper.Client;

using ApacheZooKeeper.JavaPorts;

using System.Net;

public class ZooKeeperOptions
{
    private readonly String connectString;
    private readonly int sessionTimeout;
    private readonly Watcher defaultWatcher;
    private readonly Func<IEnumerable<InetSocketAddress>, HostProvider> hostProvider;
    private readonly bool canBeReadOnly;
    private readonly long sessionId;
    private readonly byte[] sessionPasswd;
    private readonly ZKClientConfig clientConfig;

    public ZooKeeperOptions(String connectString,
        int sessionTimeout,
        Watcher defaultWatcher,
        Func<IEnumerable<InetSocketAddress>, HostProvider> hostProvider,
        bool canBeReadOnly,
        long sessionId,
        byte[] sessionPasswd,
        ZKClientConfig clientConfig) {
        this.connectString = connectString;
        this.sessionTimeout = sessionTimeout;
        this.hostProvider = hostProvider;
        this.defaultWatcher = defaultWatcher;
        this.canBeReadOnly = canBeReadOnly;
        this.sessionId = sessionId;
        this.sessionPasswd = sessionPasswd;
        this.clientConfig = clientConfig;
    }

    public String getConnectString() {
        return connectString;
    }

    public int getSessionTimeout() {
        return sessionTimeout;
    }

    public Watcher getDefaultWatcher() {
        return defaultWatcher;
    }

    public Func<IEnumerable<InetSocketAddress>, HostProvider> getHostProvider() {
        return hostProvider;
    }

    public bool isCanBeReadOnly() {
        return canBeReadOnly;
    }

    public long getSessionId() {
        return sessionId;
    }

    public byte[] getSessionPasswd() {
        return sessionPasswd;
    }

    public ZKClientConfig getClientConfig() {
        return clientConfig;
    }
}
