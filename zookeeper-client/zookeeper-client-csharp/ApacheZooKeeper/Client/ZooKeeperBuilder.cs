namespace ApacheZooKeeper.Client;

using ApacheZooKeeper.JavaPorts;

public class ZooKeeperBuilder
{
    private readonly String connectString;
    private readonly int sessionTimeout;
    private Func<IEnumerable<InetSocketAddress>, HostProvider> hostProvider;
    private Watcher defaultWatcher;
    private bool canBeReadOnly = false;
    private long sessionId = 0;
    private byte[] sessionPasswd;
    private ZKClientConfig clientConfig;

    /**
     * Creates a builder with given connect string and session timeout.
     *
     * @param connectString
     *            comma separated host:port pairs, each corresponding to a zk
     *            server. e.g. "127.0.0.1:3000,127.0.0.1:3001,127.0.0.1:3002"
     *            If the optional chroot suffix is used the example would look
     *            like: "127.0.0.1:3000,127.0.0.1:3001,127.0.0.1:3002/app/a"
     *            where the client would be rooted at "/app/a" and all paths
     *            would be relative to this root - ie getting/setting/etc...
     *            "/foo/bar" would result in operations being run on
     *            "/app/a/foo/bar" (from the server perspective).
     * @param sessionTimeoutMs
     *            session timeout in milliseconds
     */
    public ZooKeeperBuilder(String connectString, int sessionTimeoutMs) {
        this.connectString = connectString;
        this.sessionTimeout = sessionTimeoutMs;
    }

    /**
     * Specified watcher to receive state changes, and node events if attached later.
     *
     * @param watcher
     *            a watcher object which will be notified of state changes, may
     *            also be notified for node events
     * @return this
     */
    public ZooKeeperBuilder withDefaultWatcher(Watcher watcher) {
        this.defaultWatcher = watcher;
        return this;
    }

    /**
     * Specifies a function to construct a {@link HostProvider} with initial server addresses from connect string.
     *
     * @param hostProvider
     *            use this as HostProvider to enable custom behaviour.
     * @return this
     */
    public ZooKeeperBuilder withHostProvider(Func<IEnumerable<InetSocketAddress>, HostProvider> hostProvider) {
        this.hostProvider = hostProvider;
        return this;
    }

    /**
     * Specifies whether the created client is allowed to go to read-only mode in case of partitioning.
     *
     * @param canBeReadOnly
     *            whether the created client is allowed to go to
     *            read-only mode in case of partitioning. Read-only mode
     *            basically means that if the client can't find any majority
     *            servers but there's partitioned server it could reach, it
     *            connects to one in read-only mode, i.e. read requests are
     *            allowed while write requests are not. It continues seeking for
     *            majority in the background.
     * @return this
     * @since 3.4
     */
    public ZooKeeperBuilder withCanBeReadOnly(bool canBeReadOnly) {
        this.canBeReadOnly = canBeReadOnly;
        return this;
    }

    /**
     * Specifies session id and password in session reestablishment.
     *
     * @param sessionId
     *            session id to use if reconnecting, otherwise 0 to open new session
     * @param sessionPasswd
     *            password for this session
     * @return this
     * @see ZooKeeper#getSessionId()
     * @see ZooKeeper#getSessionPasswd()
     */
    public ZooKeeperBuilder withSession(long sessionId, byte[] sessionPasswd) {
        this.sessionId = sessionId;
        this.sessionPasswd = sessionPasswd;
        return this;
    }

    /**
     * Specifies the client config used to construct ZooKeeper instances.
     *
     * @param clientConfig
     *            passing this conf object gives each client the flexibility of
     *            configuring properties differently compared to other instances
     * @return this
     * @since 3.5.2
     */
    public ZooKeeperBuilder withClientConfig(ZKClientConfig clientConfig) {
        this.clientConfig = clientConfig;
        return this;
    }

    /**
     * Creates a {@link ZooKeeperOptions} with configured options.
     *
     * @apiNote helper to delegate existing constructors to {@link ZooKeeper#ZooKeeper(ZooKeeperOptions)}
     */
    public ZooKeeperOptions toOptions() {
        return new ZooKeeperOptions(
            connectString,
            sessionTimeout,
            defaultWatcher,
            hostProvider,
            canBeReadOnly,
            sessionId,
            sessionPasswd,
            clientConfig
        );
    }

    /**
     * Constructs an instance of {@link ZooKeeper}.
     *
     * @return an instance of {@link ZooKeeper}
     * @throws IOException from constructor of {@link ZooKeeper}
     */
    public ZooKeeper build() {
        return new ZooKeeper(toOptions());
    }

    /**
     * Constructs an instance of {@link ZooKeeperAdmin}.
     *
     * @return an instance of {@link ZooKeeperAdmin}
     * @throws IOException from constructor of {@link ZooKeeperAdmin}
     */
    // TODO:
    /*
    public ZooKeeperAdmin buildAdmin() {
        return new ZooKeeperAdmin(toOptions());
    }
    */
}
