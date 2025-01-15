namespace ApacheZooKeeper;

using ApacheZooKeeper.Client;
using ApacheZooKeeper.Logging;
using ApacheZooKeeper.Proto;

/**
 * This is the main class of ZooKeeper client library. To use a ZooKeeper
 * service, an application must first instantiate an object of ZooKeeper class.
 * All the iterations will be done by calling the methods of ZooKeeper class.
 * The methods of this class are thread-safe unless otherwise noted.
 * <p>
 * Once a connection to a server is established, a session ID is assigned to the
 * client. The client will send heart beats to the server periodically to keep
 * the session valid.
 * <p>
 * The application can call ZooKeeper APIs through a client as long as the
 * session ID of the client remains valid.
 * <p>
 * If for some reason, the client fails to send heart beats to the server for a
 * prolonged period of time (exceeding the sessionTimeout value, for instance),
 * the server will expire the session, and the session ID will become invalid.
 * The client object will no longer be usable. To make ZooKeeper API calls, the
 * application must create a new client object.
 * <p>
 * If the ZooKeeper server the client currently connects to fails or otherwise
 * does not respond, the client will automatically try to connect to another
 * server before its session ID expires. If successful, the application can
 * continue to use the client.
 * <p>
 * The ZooKeeper API methods are either synchronous or asynchronous. Synchronous
 * methods blocks until the server has responded. Asynchronous methods just queue
 * the request for sending and return immediately. They take a callback object that
 * will be executed either on successful execution of the request or on error with
 * an appropriate return code (rc) indicating the error.
 * <p>
 * Some successful ZooKeeper API calls can leave watches on the "data nodes" in
 * the ZooKeeper server. Other successful ZooKeeper API calls can trigger those
 * watches. Once a watch is triggered, an event will be delivered to the client
 * which left the watch at the first place. Each watch can be triggered only
 * once. Thus, up to one event will be delivered to a client for every watch it
 * leaves.
 * <p>
 * A client needs an object of a class implementing Watcher interface for
 * processing the events delivered to the client.
 *
 * When a client drops the current connection and re-connects to a server, all the
 * existing watches are considered as being triggered but the undelivered events
 * are lost. To emulate this, the client will generate a special event to tell
 * the event handler a connection has been dropped. This special event has
 * EventType None and KeeperState Disconnected.
 *
 */
public class ZooKeeper : IDisposable
{

    /**
     * @deprecated Use {@link ZKClientConfig#ZOOKEEPER_CLIENT_CNXN_SOCKET}
     *             instead.
     */
    [Obsolete]
    public static readonly String ZOOKEEPER_CLIENT_CNXN_SOCKET = "zookeeper.clientCnxnSocket";
    // Setting this to "true" will enable encrypted client-server communication.

    /**
     * @deprecated Use {@link ZKClientConfig#SECURE_CLIENT}
     *             instead.
     */
    [Obsolete]
    public static readonly String SECURE_CLIENT = "zookeeper.client.secure";

    protected readonly ClientCnxn cnxn;
    private static readonly ILogger LOG;

    static ZooKeeper() {
        //Keep these two lines together to keep the initialization order explicit
        LOG = LoggerManager.GetLogger<ZooKeeper>();
        //Environment.logEnv("Client environment:", LOG);
    }

    protected readonly HostProvider hostProvider;

    internal readonly Chroot chroot;

    /**
     * This function allows a client to update the connection string by providing
     * a new comma separated list of host:port pairs, each corresponding to a
     * ZooKeeper server.
     * <p>
     * The function invokes a <a href="https://issues.apache.org/jira/browse/ZOOKEEPER-1355">
     * probabilistic load-balancing algorithm</a> which may cause the client to disconnect from
     * its current host with the goal to achieve expected uniform number of connections per server
     * in the new list. In case the current host to which the client is connected is not in the new
     * list this call will always cause the connection to be dropped. Otherwise, the decision
     * is based on whether the number of servers has increased or decreased and by how much.
     * For example, if the previous connection string contained 3 hosts and now the list contains
     * these 3 hosts and 2 more hosts, 40% of clients connected to each of the 3 hosts will
     * move to one of the new hosts in order to balance the load. The algorithm will disconnect
     * from the current host with probability 0.4 and in this case cause the client to connect
     * to one of the 2 new hosts, chosen at random.
     * <p>
     * If the connection is dropped, the client moves to a special mode "reconfigMode" where he chooses
     * a new server to connect to using the probabilistic algorithm. After finding a server,
     * or exhausting all servers in the new list after trying all of them and failing to connect,
     * the client moves back to the normal mode of operation where it will pick an arbitrary server
     * from the connectString and attempt to connect to it. If establishment of
     * the connection fails, another server in the connect string will be tried
     * (the order is non-deterministic, as we random shuffle the list), until a
     * connection is established. The client will continue attempts until the
     * session is explicitly closed (or the session is expired by the server).
     * @param connectString
     *            comma separated host:port pairs, each corresponding to a zk
     *            server. e.g. "127.0.0.1:3000,127.0.0.1:3001,127.0.0.1:3002"
     *            If the optional chroot suffix is used the example would look
     *            like: "127.0.0.1:3000,127.0.0.1:3001,127.0.0.1:3002/app/a"
     *            where the client would be rooted at "/app/a" and all paths
     *            would be relative to this root - ie getting/setting/etc...
     *            "/foo/bar" would result in operations being run on
     *            "/app/a/foo/bar" (from the server perspective).
     *
     * @throws IOException in cases of network failure
     */
    //TODO:
    /*
    public void updateServerList(String connectString) {
        ConnectStringParser connectStringParser = new ConnectStringParser(connectString);
        Collection<InetSocketAddress> serverAddresses = connectStringParser.getServerAddresses();

        ClientCnxnSocket clientCnxnSocket = cnxn.sendThread.getClientCnxnSocket();
        InetSocketAddress currentHost = (InetSocketAddress) clientCnxnSocket.getRemoteSocketAddress();

        bool reconfigMode = hostProvider.updateServerList(serverAddresses, currentHost);

        // cause disconnection - this will cause next to be called
        // which will in turn call nextReconfigMode
        if (reconfigMode) {
            clientCnxnSocket.testableCloseSocket();
        }
    }

    public ZooKeeperSaslClient getSaslClient() {
        return cnxn.getZooKeeperSaslClient();
    }
    */

    private readonly ZKClientConfig clientConfig;

    public ZKClientConfig getClientConfig() {
        return clientConfig;
    }

    protected List<String> getDataWatches() {
        return getWatchManager().getDataWatchList();
    }

    protected List<String> getExistWatches() {
        return getWatchManager().getExistWatchList();
    }

    protected List<String> getChildWatches() {
        return getWatchManager().getChildWatchList();
    }

    protected List<String> getPersistentWatches() {
        return getWatchManager().getPersistentWatchList();
    }

    protected List<String> getPersistentRecursiveWatches() {
        return getWatchManager().getPersistentRecursiveWatchList();
    }

    internal ZKWatchManager getWatchManager() {
        return cnxn.getWatcherManager();
    }

    /**
     * Register a watcher for a particular path.
     */
    internal abstract class WatchRegistration {

        private Watcher watcher;
        private String serverPath;

        public WatchRegistration(Watcher watcher, String serverPath) {
            this.watcher = watcher;
            this.serverPath = serverPath;
        }

        protected abstract Dictionary<String, HashSet<Watcher>> getWatches(int rc);

        /**
         * Register the watcher with the set of watches on path.
         * @param rc the result code of the operation that attempted to
         * add the watch on the path.
         */
        public void register(int rc) {
            if (shouldAddWatch(rc)) {
                Dictionary<String, HashSet<Watcher>> watches = getWatches(rc);
                lock (watches) {
                    HashSet<Watcher> watchers = watches.get(serverPath);
                    if (watchers == null) {
                        watchers = new();
                        watches.put(serverPath, watchers);
                    }
                    watchers.add(watcher);
                }
            }
        }
        /**
         * Determine whether the watch should be added based on return code.
         * @param rc the result code of the operation that attempted to add the
         * watch on the node
         * @return true if the watch should be added, otw false
         */
        protected virtual bool shouldAddWatch(int rc) {
            return rc == KeeperException.Code.OK.intValue();
        }

    }

    /** Handle the special case of exists watches - they add a watcher
     * even in the case where NONODE result code is returned.
     */
    internal class ExistsWatchRegistration : WatchRegistration {
        private readonly ZooKeeper _zk;

        public ExistsWatchRegistration(ZooKeeper zk, Watcher watcher, String clientPath)
            : base(zk.chroot.interceptWatcher(watcher), zk.prependChroot(clientPath))
        {
            _zk = zk;
        }

        protected override Dictionary<String, HashSet<Watcher>> getWatches(int rc) {
            return rc == KeeperException.Code.OK.intValue()
                    ? _zk.getWatchManager().getDataWatches() : _zk.getWatchManager().getExistWatches();
        }

        protected override bool shouldAddWatch(int rc) {
            return rc == KeeperException.Code.OK.intValue() || rc == KeeperException.Code.NONODE.intValue();
        }

    }

    class ServerDataWatchRegistration : WatchRegistration
    {
        private readonly ZooKeeper _zk;

        public ServerDataWatchRegistration(ZooKeeper zk, Watcher watcher, String serverPath)
            : base(watcher, serverPath)
        {
            _zk = zk;
        }

        protected override Dictionary<String, HashSet<Watcher>> getWatches(int rc) {
            return _zk.getWatchManager().getDataWatches();
        }

    }

    class DataWatchRegistration : WatchRegistration {
        private readonly ZooKeeper _zk;

        public DataWatchRegistration(ZooKeeper zk, Watcher watcher, String clientPath)
            : base(zk.chroot.interceptWatcher(watcher), zk.prependChroot(clientPath))
        {
            _zk = zk;
        }

        protected override Dictionary<String, HashSet<Watcher>> getWatches(int rc) {
            return _zk.getWatchManager().getDataWatches();
        }

    }

    class ChildWatchRegistration : WatchRegistration {
        private readonly ZooKeeper _zk;

        public ChildWatchRegistration(ZooKeeper zk, Watcher watcher, String clientPath)
            : base(zk.chroot.interceptWatcher(watcher), zk.prependChroot(clientPath))
        {
            _zk = zk;
        }

        protected override Dictionary<String, HashSet<Watcher>> getWatches(int rc) {
            return _zk.getWatchManager().getChildWatches();
        }

    }

    // TODO:
    /*
    class AddWatchRegistration : WatchRegistration {
        private readonly AddWatchMode mode;

        public AddWatchRegistration(Watcher watcher, String clientPath, AddWatchMode mode) {
            base(chroot.interceptWatcher(watcher), prependChroot(clientPath));
            this.mode = mode;
        }

        @Override
        protected Map<String, Set<Watcher>> getWatches(int rc) {
            switch (mode) {
                case PERSISTENT:
                    return getWatchManager().getPersistentWatches();
                case PERSISTENT_RECURSIVE:
                    return getWatchManager().getPersistentRecursiveWatches();
            }
            throw new IllegalArgumentException("Mode not supported: " + mode);
        }

        @Override
        protected bool shouldAddWatch(int rc) {
            return rc == KeeperException.Code.OK.intValue() || rc == KeeperException.Code.NONODE.intValue();
        }
    }
    */

    public enum States
    {
        CONNECTING,
        ASSOCIATING,
        CONNECTED,
        CONNECTEDREADONLY,
        CLOSED,
        AUTH_FAILED,
        NOT_CONNECTED,
    }

    /**
     * To create a ZooKeeper client object, the application needs to pass a
     * connection string containing a comma separated list of host:port pairs,
     * each corresponding to a ZooKeeper server.
     * <p>
     * Session establishment is asynchronous. This constructor will initiate
     * connection to the server and return immediately - potentially (usually)
     * before the session is fully established. The watcher argument specifies
     * the watcher that will be notified of any changes in state. This
     * notification can come at any point before or after the constructor call
     * has returned.
     * <p>
     * The instantiated ZooKeeper client object will pick an arbitrary server
     * from the connectString and attempt to connect to it. If establishment of
     * the connection fails, another server in the connect string will be tried
     * (the order is non-deterministic, as we random shuffle the list), until a
     * connection is established. The client will continue attempts until the
     * session is explicitly closed.
     * <p>
     * Added in 3.2.0: An optional "chroot" suffix may also be appended to the
     * connection string. This will run the client commands while interpreting
     * all paths relative to this root (similar to the unix chroot command).
     *
     * @param connectString
     *            comma separated host:port pairs, each corresponding to a zk
     *            server. e.g. "127.0.0.1:3000,127.0.0.1:3001,127.0.0.1:3002" If
     *            the optional chroot suffix is used the example would look
     *            like: "127.0.0.1:3000,127.0.0.1:3001,127.0.0.1:3002/app/a"
     *            where the client would be rooted at "/app/a" and all paths
     *            would be relative to this root - ie getting/setting/etc...
     *            "/foo/bar" would result in operations being run on
     *            "/app/a/foo/bar" (from the server perspective).
     * @param sessionTimeout
     *            session timeout in milliseconds
     * @param watcher
     *            a watcher object which will be notified of state changes, may
     *            also be notified for node events
     *
     * @throws IOException
     *             in cases of network failure
     * @throws IllegalArgumentException
     *             if an invalid chroot path is specified
     */
    public ZooKeeper(String connectString, int sessionTimeout, Watcher watcher)
        : this(new ZooKeeperBuilder(connectString, sessionTimeout)
            .withDefaultWatcher(watcher)
            .toOptions())
    {
    }

    /**
     * To create a ZooKeeper client object, the application needs to pass a
     * connection string containing a comma separated list of host:port pairs,
     * each corresponding to a ZooKeeper server.
     * <p>
     * Session establishment is asynchronous. This constructor will initiate
     * connection to the server and return immediately - potentially (usually)
     * before the session is fully established. The watcher argument specifies
     * the watcher that will be notified of any changes in state. This
     * notification can come at any point before or after the constructor call
     * has returned.
     * <p>
     * The instantiated ZooKeeper client object will pick an arbitrary server
     * from the connectString and attempt to connect to it. If establishment of
     * the connection fails, another server in the connect string will be tried
     * (the order is non-deterministic, as we random shuffle the list), until a
     * connection is established. The client will continue attempts until the
     * session is explicitly closed.
     * <p>
     * Added in 3.2.0: An optional "chroot" suffix may also be appended to the
     * connection string. This will run the client commands while interpreting
     * all paths relative to this root (similar to the unix chroot command).
     *
     * @param connectString
     *            comma separated host:port pairs, each corresponding to a zk
     *            server. e.g. "127.0.0.1:3000,127.0.0.1:3001,127.0.0.1:3002" If
     *            the optional chroot suffix is used the example would look
     *            like: "127.0.0.1:3000,127.0.0.1:3001,127.0.0.1:3002/app/a"
     *            where the client would be rooted at "/app/a" and all paths
     *            would be relative to this root - ie getting/setting/etc...
     *            "/foo/bar" would result in operations being run on
     *            "/app/a/foo/bar" (from the server perspective).
     * @param sessionTimeout
     *            session timeout in milliseconds
     * @param watcher
     *            a watcher object which will be notified of state changes, may
     *            also be notified for node events
     * @param conf
     *            (added in 3.5.2) passing this conf object gives each client the flexibility of
     *            configuring properties differently compared to other instances
     * @throws IOException
     *             in cases of network failure
     * @throws IllegalArgumentException
     *             if an invalid chroot path is specified
     */
    public ZooKeeper(
        String connectString,
        int sessionTimeout,
        Watcher watcher,
        ZKClientConfig conf)
        : this(new ZooKeeperBuilder(connectString, sessionTimeout)
            .withDefaultWatcher(watcher)
            .withClientConfig(conf)
            .toOptions())
    {
    }

    /**
     * To create a ZooKeeper client object, the application needs to pass a
     * connection string containing a comma separated list of host:port pairs,
     * each corresponding to a ZooKeeper server.
     * <p>
     * Session establishment is asynchronous. This constructor will initiate
     * connection to the server and return immediately - potentially (usually)
     * before the session is fully established. The watcher argument specifies
     * the watcher that will be notified of any changes in state. This
     * notification can come at any point before or after the constructor call
     * has returned.
     * <p>
     * The instantiated ZooKeeper client object will pick an arbitrary server
     * from the connectString and attempt to connect to it. If establishment of
     * the connection fails, another server in the connect string will be tried
     * (the order is non-deterministic, as we random shuffle the list), until a
     * connection is established. The client will continue attempts until the
     * session is explicitly closed.
     * <p>
     * Added in 3.2.0: An optional "chroot" suffix may also be appended to the
     * connection string. This will run the client commands while interpreting
     * all paths relative to this root (similar to the unix chroot command).
     * <p>
     * For backward compatibility, there is another version
     * {@link #ZooKeeper(String, int, Watcher, bool)} which uses
     * default {@link StaticHostProvider}
     *
     * @param connectString
     *            comma separated host:port pairs, each corresponding to a zk
     *            server. e.g. "127.0.0.1:3000,127.0.0.1:3001,127.0.0.1:3002" If
     *            the optional chroot suffix is used the example would look
     *            like: "127.0.0.1:3000,127.0.0.1:3001,127.0.0.1:3002/app/a"
     *            where the client would be rooted at "/app/a" and all paths
     *            would be relative to this root - ie getting/setting/etc...
     *            "/foo/bar" would result in operations being run on
     *            "/app/a/foo/bar" (from the server perspective).
     * @param sessionTimeout
     *            session timeout in milliseconds
     * @param watcher
     *            a watcher object which will be notified of state changes, may
     *            also be notified for node events
     * @param canBeReadOnly
     *            (added in 3.4) whether the created client is allowed to go to
     *            read-only mode in case of partitioning. Read-only mode
     *            basically means that if the client can't find any majority
     *            servers but there's partitioned server it could reach, it
     *            connects to one in read-only mode, i.e. read requests are
     *            allowed while write requests are not. It continues seeking for
     *            majority in the background.
     * @param aHostProvider
     *            use this as HostProvider to enable custom behaviour.
     *
     * @throws IOException
     *             in cases of network failure
     * @throws IllegalArgumentException
     *             if an invalid chroot path is specified
     */
    public ZooKeeper(
        String connectString,
        int sessionTimeout,
        Watcher watcher,
        bool canBeReadOnly,
        HostProvider aHostProvider)
        : this(new ZooKeeperBuilder(connectString, sessionTimeout)
            .withDefaultWatcher(watcher)
            .withCanBeReadOnly(canBeReadOnly)
            .withHostProvider(_ => aHostProvider)
            .toOptions())
    {
    }

    /**
     * To create a ZooKeeper client object, the application needs to pass a
     * connection string containing a comma separated list of host:port pairs,
     * each corresponding to a ZooKeeper server.
     * <p>
     * Session establishment is asynchronous. This constructor will initiate
     * connection to the server and return immediately - potentially (usually)
     * before the session is fully established. The watcher argument specifies
     * the watcher that will be notified of any changes in state. This
     * notification can come at any point before or after the constructor call
     * has returned.
     * <p>
     * The instantiated ZooKeeper client object will pick an arbitrary server
     * from the connectString and attempt to connect to it. If establishment of
     * the connection fails, another server in the connect string will be tried
     * (the order is non-deterministic, as we random shuffle the list), until a
     * connection is established. The client will continue attempts until the
     * session is explicitly closed.
     * <p>
     * Added in 3.2.0: An optional "chroot" suffix may also be appended to the
     * connection string. This will run the client commands while interpreting
     * all paths relative to this root (similar to the unix chroot command).
     * <p>
     * For backward compatibility, there is another version
     * {@link #ZooKeeper(String, int, Watcher, bool)} which uses default
     * {@link StaticHostProvider}
     *
     * @param connectString
     *            comma separated host:port pairs, each corresponding to a zk
     *            server. e.g. "127.0.0.1:3000,127.0.0.1:3001,127.0.0.1:3002" If
     *            the optional chroot suffix is used the example would look
     *            like: "127.0.0.1:3000,127.0.0.1:3001,127.0.0.1:3002/app/a"
     *            where the client would be rooted at "/app/a" and all paths
     *            would be relative to this root - ie getting/setting/etc...
     *            "/foo/bar" would result in operations being run on
     *            "/app/a/foo/bar" (from the server perspective).
     * @param sessionTimeout
     *            session timeout in milliseconds
     * @param watcher
     *            a watcher object which will be notified of state changes, may
     *            also be notified for node events
     * @param canBeReadOnly
     *            (added in 3.4) whether the created client is allowed to go to
     *            read-only mode in case of partitioning. Read-only mode
     *            basically means that if the client can't find any majority
     *            servers but there's partitioned server it could reach, it
     *            connects to one in read-only mode, i.e. read requests are
     *            allowed while write requests are not. It continues seeking for
     *            majority in the background.
     * @param hostProvider
     *            use this as HostProvider to enable custom behaviour.
     * @param clientConfig
     *            (added in 3.5.2) passing this conf object gives each client the flexibility of
     *            configuring properties differently compared to other instances
     * @throws IOException
     *             in cases of network failure
     * @throws IllegalArgumentException
     *             if an invalid chroot path is specified
     */
    public ZooKeeper(
        String connectString,
        int sessionTimeout,
        Watcher watcher,
        bool canBeReadOnly,
        HostProvider hostProvider,
        ZKClientConfig clientConfig
    )
        : this(new ZooKeeperBuilder(connectString, sessionTimeout)
            .withDefaultWatcher(watcher)
            .withCanBeReadOnly(canBeReadOnly)
            .withHostProvider(_ => hostProvider)
            .withClientConfig(clientConfig)
            .toOptions())
    {
    }


    ClientCnxn createConnection(
        HostProvider hostProvider,
        int sessionTimeout,
        ZKClientConfig clientConfig,
        Watcher defaultWatcher,
        ClientCnxnSocket clientCnxnSocket,
        long sessionId,
        byte[] sessionPasswd,
        bool canBeReadOnly
    ) {
        return new ClientCnxn(
            hostProvider,
            sessionTimeout,
            clientConfig,
            defaultWatcher,
            clientCnxnSocket,
            sessionId,
            sessionPasswd,
            canBeReadOnly);
    }


    /**
     * To create a ZooKeeper client object, the application needs to pass a
     * connection string containing a comma separated list of host:port pairs,
     * each corresponding to a ZooKeeper server.
     * <p>
     * Session establishment is asynchronous. This constructor will initiate
     * connection to the server and return immediately - potentially (usually)
     * before the session is fully established. The watcher argument specifies
     * the watcher that will be notified of any changes in state. This
     * notification can come at any point before or after the constructor call
     * has returned.
     * <p>
     * The instantiated ZooKeeper client object will pick an arbitrary server
     * from the connectString and attempt to connect to it. If establishment of
     * the connection fails, another server in the connect string will be tried
     * (the order is non-deterministic, as we random shuffle the list), until a
     * connection is established. The client will continue attempts until the
     * session is explicitly closed.
     * <p>
     * Added in 3.2.0: An optional "chroot" suffix may also be appended to the
     * connection string. This will run the client commands while interpreting
     * all paths relative to this root (similar to the unix chroot command).
     * <p>
     *
     * @param connectString
     *            comma separated host:port pairs, each corresponding to a zk
     *            server. e.g. "127.0.0.1:3000,127.0.0.1:3001,127.0.0.1:3002" If
     *            the optional chroot suffix is used the example would look
     *            like: "127.0.0.1:3000,127.0.0.1:3001,127.0.0.1:3002/app/a"
     *            where the client would be rooted at "/app/a" and all paths
     *            would be relative to this root - ie getting/setting/etc...
     *            "/foo/bar" would result in operations being run on
     *            "/app/a/foo/bar" (from the server perspective).
     * @param sessionTimeout
     *            session timeout in milliseconds
     * @param watcher
     *            a watcher object which will be notified of state changes, may
     *            also be notified for node events
     * @param canBeReadOnly
     *            (added in 3.4) whether the created client is allowed to go to
     *            read-only mode in case of partitioning. Read-only mode
     *            basically means that if the client can't find any majority
     *            servers but there's partitioned server it could reach, it
     *            connects to one in read-only mode, i.e. read requests are
     *            allowed while write requests are not. It continues seeking for
     *            majority in the background.
     *
     * @throws IOException
     *             in cases of network failure
     * @throws IllegalArgumentException
     *             if an invalid chroot path is specified
     */
    public ZooKeeper(
        String connectString,
        int sessionTimeout,
        Watcher watcher,
        bool canBeReadOnly)
        : this(new ZooKeeperBuilder(connectString, sessionTimeout)
            .withDefaultWatcher(watcher)
            .withCanBeReadOnly(canBeReadOnly)
            .toOptions())
    {
    }

    /**
     * To create a ZooKeeper client object, the application needs to pass a
     * connection string containing a comma separated list of host:port pairs,
     * each corresponding to a ZooKeeper server.
     * <p>
     * Session establishment is asynchronous. This constructor will initiate
     * connection to the server and return immediately - potentially (usually)
     * before the session is fully established. The watcher argument specifies
     * the watcher that will be notified of any changes in state. This
     * notification can come at any point before or after the constructor call
     * has returned.
     * <p>
     * The instantiated ZooKeeper client object will pick an arbitrary server
     * from the connectString and attempt to connect to it. If establishment of
     * the connection fails, another server in the connect string will be tried
     * (the order is non-deterministic, as we random shuffle the list), until a
     * connection is established. The client will continue attempts until the
     * session is explicitly closed.
     * <p>
     * Added in 3.2.0: An optional "chroot" suffix may also be appended to the
     * connection string. This will run the client commands while interpreting
     * all paths relative to this root (similar to the unix chroot command).
     * <p>
     *
     * @param connectString
     *            comma separated host:port pairs, each corresponding to a zk
     *            server. e.g. "127.0.0.1:3000,127.0.0.1:3001,127.0.0.1:3002" If
     *            the optional chroot suffix is used the example would look
     *            like: "127.0.0.1:3000,127.0.0.1:3001,127.0.0.1:3002/app/a"
     *            where the client would be rooted at "/app/a" and all paths
     *            would be relative to this root - ie getting/setting/etc...
     *            "/foo/bar" would result in operations being run on
     *            "/app/a/foo/bar" (from the server perspective).
     * @param sessionTimeout
     *            session timeout in milliseconds
     * @param watcher
     *            a watcher object which will be notified of state changes, may
     *            also be notified for node events
     * @param canBeReadOnly
     *            (added in 3.4) whether the created client is allowed to go to
     *            read-only mode in case of partitioning. Read-only mode
     *            basically means that if the client can't find any majority
     *            servers but there's partitioned server it could reach, it
     *            connects to one in read-only mode, i.e. read requests are
     *            allowed while write requests are not. It continues seeking for
     *            majority in the background.
     * @param conf
     *            (added in 3.5.2) passing this conf object gives each client the flexibility of
     *            configuring properties differently compared to other instances
     * @throws IOException
     *             in cases of network failure
     * @throws IllegalArgumentException
     *             if an invalid chroot path is specified
     */
    public ZooKeeper(
        String connectString,
        int sessionTimeout,
        Watcher watcher,
        bool canBeReadOnly,
        ZKClientConfig conf)
        : this(new ZooKeeperBuilder(connectString, sessionTimeout)
            .withDefaultWatcher(watcher)
            .withCanBeReadOnly(canBeReadOnly)
            .withClientConfig(conf)
            .toOptions())
    {
    }

    /**
     * To create a ZooKeeper client object, the application needs to pass a
     * connection string containing a comma separated list of host:port pairs,
     * each corresponding to a ZooKeeper server.
     * <p>
     * Session establishment is asynchronous. This constructor will initiate
     * connection to the server and return immediately - potentially (usually)
     * before the session is fully established. The watcher argument specifies
     * the watcher that will be notified of any changes in state. This
     * notification can come at any point before or after the constructor call
     * has returned.
     * <p>
     * The instantiated ZooKeeper client object will pick an arbitrary server
     * from the connectString and attempt to connect to it. If establishment of
     * the connection fails, another server in the connect string will be tried
     * (the order is non-deterministic, as we random shuffle the list), until a
     * connection is established. The client will continue attempts until the
     * session is explicitly closed (or the session is expired by the server).
     * <p>
     * Added in 3.2.0: An optional "chroot" suffix may also be appended to the
     * connection string. This will run the client commands while interpreting
     * all paths relative to this root (similar to the unix chroot command).
     * <p>
     * Use {@link #getSessionId} and {@link #getSessionPasswd} on an established
     * client connection, these values must be passed as sessionId and
     * sessionPasswd respectively if reconnecting. Otherwise, if not
     * reconnecting, use the other constructor which does not require these
     * parameters.
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
     * @param sessionTimeout
     *            session timeout in milliseconds
     * @param watcher
     *            a watcher object which will be notified of state changes, may
     *            also be notified for node events
     * @param sessionId
     *            specific session id to use if reconnecting
     * @param sessionPasswd
     *            password for this session
     *
     * @throws IOException in cases of network failure
     * @throws IllegalArgumentException if an invalid chroot path is specified
     * @throws IllegalArgumentException for an invalid list of ZooKeeper hosts
     */
    public ZooKeeper(
        String connectString,
        int sessionTimeout,
        Watcher watcher,
        long sessionId,
        byte[] sessionPasswd)
        : this(new ZooKeeperBuilder(connectString, sessionTimeout)
            .withDefaultWatcher(watcher)
            .withSession(sessionId, sessionPasswd)
            .toOptions())
    {
    }

    /**
     * To create a ZooKeeper client object, the application needs to pass a
     * connection string containing a comma separated list of host:port pairs,
     * each corresponding to a ZooKeeper server.
     * <p>
     * Session establishment is asynchronous. This constructor will initiate
     * connection to the server and return immediately - potentially (usually)
     * before the session is fully established. The watcher argument specifies
     * the watcher that will be notified of any changes in state. This
     * notification can come at any point before or after the constructor call
     * has returned.
     * <p>
     * The instantiated ZooKeeper client object will pick an arbitrary server
     * from the connectString and attempt to connect to it. If establishment of
     * the connection fails, another server in the connect string will be tried
     * (the order is non-deterministic, as we random shuffle the list), until a
     * connection is established. The client will continue attempts until the
     * session is explicitly closed (or the session is expired by the server).
     * <p>
     * Added in 3.2.0: An optional "chroot" suffix may also be appended to the
     * connection string. This will run the client commands while interpreting
     * all paths relative to this root (similar to the unix chroot command).
     * <p>
     * Use {@link #getSessionId} and {@link #getSessionPasswd} on an established
     * client connection, these values must be passed as sessionId and
     * sessionPasswd respectively if reconnecting. Otherwise, if not
     * reconnecting, use the other constructor which does not require these
     * parameters.
     * <p>
     * For backward compatibility, there is another version
     * {@link #ZooKeeper(String, int, Watcher, long, byte[], bool)} which uses
     * default {@link StaticHostProvider}
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
     * @param sessionTimeout
     *            session timeout in milliseconds
     * @param watcher
     *            a watcher object which will be notified of state changes, may
     *            also be notified for node events
     * @param sessionId
     *            specific session id to use if reconnecting
     * @param sessionPasswd
     *            password for this session
     * @param canBeReadOnly
     *            (added in 3.4) whether the created client is allowed to go to
     *            read-only mode in case of partitioning. Read-only mode
     *            basically means that if the client can't find any majority
     *            servers but there's partitioned server it could reach, it
     *            connects to one in read-only mode, i.e. read requests are
     *            allowed while write requests are not. It continues seeking for
     *            majority in the background.
     * @param aHostProvider
     *            use this as HostProvider to enable custom behaviour.
     * @throws IOException in cases of network failure
     * @throws IllegalArgumentException if an invalid chroot path is specified
     */
    public ZooKeeper(
        String connectString,
        int sessionTimeout,
        Watcher watcher,
        long sessionId,
        byte[] sessionPasswd,
        bool canBeReadOnly,
        HostProvider aHostProvider)
        : this(new ZooKeeperBuilder(connectString, sessionTimeout)
            .withDefaultWatcher(watcher)
            .withSession(sessionId, sessionPasswd)
            .withCanBeReadOnly(canBeReadOnly)
            .withHostProvider(_ => aHostProvider)
            .toOptions())
    {
    }

    /**
     * To create a ZooKeeper client object, the application needs to pass a
     * connection string containing a comma separated list of host:port pairs,
     * each corresponding to a ZooKeeper server.
     * <p>
     * Session establishment is asynchronous. This constructor will initiate
     * connection to the server and return immediately - potentially (usually)
     * before the session is fully established. The watcher argument specifies
     * the watcher that will be notified of any changes in state. This
     * notification can come at any point before or after the constructor call
     * has returned.
     * <p>
     * The instantiated ZooKeeper client object will pick an arbitrary server
     * from the connectString and attempt to connect to it. If establishment of
     * the connection fails, another server in the connect string will be tried
     * (the order is non-deterministic, as we random shuffle the list), until a
     * connection is established. The client will continue attempts until the
     * session is explicitly closed (or the session is expired by the server).
     * <p>
     * Added in 3.2.0: An optional "chroot" suffix may also be appended to the
     * connection string. This will run the client commands while interpreting
     * all paths relative to this root (similar to the unix chroot command).
     * <p>
     * Use {@link #getSessionId} and {@link #getSessionPasswd} on an established
     * client connection, these values must be passed as sessionId and
     * sessionPasswd respectively if reconnecting. Otherwise, if not
     * reconnecting, use the other constructor which does not require these
     * parameters.
     * <p>
     * For backward compatibility, there is another version
     * {@link #ZooKeeper(String, int, Watcher, long, byte[], bool)} which uses
     * default {@link StaticHostProvider}
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
     * @param sessionTimeout
     *            session timeout in milliseconds
     * @param watcher
     *            a watcher object which will be notified of state changes, may
     *            also be notified for node events
     * @param sessionId
     *            specific session id to use if reconnecting
     * @param sessionPasswd
     *            password for this session
     * @param canBeReadOnly
     *            (added in 3.4) whether the created client is allowed to go to
     *            read-only mode in case of partitioning. Read-only mode
     *            basically means that if the client can't find any majority
     *            servers but there's partitioned server it could reach, it
     *            connects to one in read-only mode, i.e. read requests are
     *            allowed while write requests are not. It continues seeking for
     *            majority in the background.
     * @param hostProvider
     *            use this as HostProvider to enable custom behaviour.
     * @param clientConfig
     *            (added in 3.5.2) passing this conf object gives each client the flexibility of
     *            configuring properties differently compared to other instances
     * @throws IOException in cases of network failure
     * @throws IllegalArgumentException if an invalid chroot path is specified
     *
     * @since 3.5.5
     */
    public ZooKeeper(
        String connectString,
        int sessionTimeout,
        Watcher watcher,
        long sessionId,
        byte[] sessionPasswd,
        bool canBeReadOnly,
        HostProvider hostProvider,
        ZKClientConfig clientConfig)
        : this(new ZooKeeperBuilder(connectString, sessionTimeout)
            .withSession(sessionId, sessionPasswd)
            .withDefaultWatcher(watcher)
            .withCanBeReadOnly(canBeReadOnly)
            .withHostProvider(_ => hostProvider)
            .withClientConfig(clientConfig)
            .toOptions())
    {
    }


    /**
     * Create a ZooKeeper client and establish session asynchronously.
     *
     * <p>This constructor will initiate connection to the server and return
     * immediately - potentially (usually) before the session is fully established.
     * The watcher from options will be notified of any changes in state. This
     * notification can come at any point before or after the constructor call
     * has returned.
     *
     * <p>The instantiated ZooKeeper client object will pick an arbitrary server
     * from the connect string and attempt to connect to it. If establishment of
     * the connection fails, another server in the connect string will be tried
     * (the order is non-deterministic, as we random shuffle the list), until a
     * connection is established. The client will continue attempts until the
     * session is explicitly closed (or the session is expired by the server).
     *
     * @param options options for ZooKeeper client
     * @throws IOException in cases of IO failure
     */
    public ZooKeeper(ZooKeeperOptions options) {
        String connectString = options.getConnectString();
        int sessionTimeout = options.getSessionTimeout();
        long sessionId = options.getSessionId();
        byte[] sessionPasswd = sessionId == 0 ? new byte[16] : options.getSessionPasswd();
        Watcher watcher = options.getDefaultWatcher();
        bool canBeReadOnly = options.isCanBeReadOnly();

        if (sessionId == 0) {
            LOG.info(
                "Initiating client connection, connectString={} sessionTimeout={} watcher={}",
                connectString,
                sessionTimeout,
                watcher);
        } else {
            LOG.info(
                "Initiating client connection, connectString={} "
                    + "sessionTimeout={} watcher={} sessionId=0x{} sessionPasswd={}",
                connectString,
                sessionTimeout,
                watcher,
                Long.toHexString(sessionId),
                (sessionPasswd == null ? "<null>" : "<hidden>"));
        }

        ZKClientConfig clientConfig = options.getClientConfig();
        this.clientConfig = clientConfig != null ? clientConfig : new ZKClientConfig();
        ConnectStringParser connectStringParser = new ConnectStringParser(connectString);
        HostProvider hostProvider;
        if (options.getHostProvider() != null) {
            hostProvider = options.getHostProvider().apply(connectStringParser.getServerAddresses());
        } else {
            hostProvider = new StaticHostProvider(connectStringParser.getServerAddresses());
        }
        this.hostProvider = hostProvider;

        // TODO:
        /*
        chroot = Chroot.ofNullable(connectStringParser.getChrootPath());
        cnxn = createConnection(
            hostProvider,
            sessionTimeout,
            this.clientConfig,
            watcher,
            getClientCnxnSocket(),
            sessionId,
            sessionPasswd,
            canBeReadOnly);
        cnxn.seenRwServerBefore = sessionId != 0; // since user has provided sessionId
        cnxn.start();
        */
    }

    /**
     * To create a ZooKeeper client object, the application needs to pass a
     * connection string containing a comma separated list of host:port pairs,
     * each corresponding to a ZooKeeper server.
     * <p>
     * Session establishment is asynchronous. This constructor will initiate
     * connection to the server and return immediately - potentially (usually)
     * before the session is fully established. The watcher argument specifies
     * the watcher that will be notified of any changes in state. This
     * notification can come at any point before or after the constructor call
     * has returned.
     * <p>
     * The instantiated ZooKeeper client object will pick an arbitrary server
     * from the connectString and attempt to connect to it. If establishment of
     * the connection fails, another server in the connect string will be tried
     * (the order is non-deterministic, as we random shuffle the list), until a
     * connection is established. The client will continue attempts until the
     * session is explicitly closed (or the session is expired by the server).
     * <p>
     * Added in 3.2.0: An optional "chroot" suffix may also be appended to the
     * connection string. This will run the client commands while interpreting
     * all paths relative to this root (similar to the unix chroot command).
     * <p>
     * Use {@link #getSessionId} and {@link #getSessionPasswd} on an established
     * client connection, these values must be passed as sessionId and
     * sessionPasswd respectively if reconnecting. Otherwise, if not
     * reconnecting, use the other constructor which does not require these
     * parameters.
     * <p>
     * This constructor uses a StaticHostProvider; there is another one
     * to enable custom behaviour.
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
     * @param sessionTimeout
     *            session timeout in milliseconds
     * @param watcher
     *            a watcher object which will be notified of state changes, may
     *            also be notified for node events
     * @param sessionId
     *            specific session id to use if reconnecting
     * @param sessionPasswd
     *            password for this session
     * @param canBeReadOnly
     *            (added in 3.4) whether the created client is allowed to go to
     *            read-only mode in case of partitioning. Read-only mode
     *            basically means that if the client can't find any majority
     *            servers but there's partitioned server it could reach, it
     *            connects to one in read-only mode, i.e. read requests are
     *            allowed while write requests are not. It continues seeking for
     *            majority in the background.
     * @throws IOException in cases of network failure
     * @throws IllegalArgumentException if an invalid chroot path is specified
     */
    public ZooKeeper(
        String connectString,
        int sessionTimeout,
        Watcher watcher,
        long sessionId,
        byte[] sessionPasswd,
        bool canBeReadOnly)
        : this(new ZooKeeperBuilder(connectString, sessionTimeout)
            .withDefaultWatcher(watcher)
            .withSession(sessionId, sessionPasswd)
            .withCanBeReadOnly(canBeReadOnly)
            .toOptions())
    {
    }

    // TODO:
    /*
    // VisibleForTesting
    public Testable getTestable() {
        return new ZooKeeperTestable(cnxn);
    }
    */

    /**
     * The session id for this ZooKeeper client instance. The value returned is
     * not valid until the client connects to a server and may change after a
     * re-connect.
     *
     * This method is NOT thread safe
     *
     * @return current session id
     */
    public long getSessionId() {
        return cnxn.getSessionId();
    }

    /**
     * The session password for this ZooKeeper client instance. The value
     * returned is not valid until the client connects to a server and may
     * change after a re-connect.
     *
     * This method is NOT thread safe
     *
     * @return current session password
     */
    public byte[] getSessionPasswd() {
        return cnxn.getSessionPasswd();
    }

    /**
     * The negotiated session timeout for this ZooKeeper client instance. The
     * value returned is not valid until the client connects to a server and
     * may change after a re-connect.
     *
     * This method is NOT thread safe
     *
     * @return current session timeout
     */
    public int getSessionTimeout() {
        return cnxn.getSessionTimeout();
    }

    /**
     * Add the specified scheme:auth information to this connection.
     *
     * This method is NOT thread safe
     *
     * @param scheme
     * @param auth
     */
    public void addAuthInfo(String scheme, byte[] auth) {
        cnxn.addAuthInfo(scheme, auth);
    }

    /**
     * Specify the default watcher for the connection (overrides the one
     * specified during construction).
     */
    public void register(Watcher watcher) {
        lock (this) {
            getWatchManager().setDefaultWatcher(watcher);
        }
    }

    /**
     * Close this client object. Once the client is closed, its session becomes
     * invalid. All the ephemeral nodes in the ZooKeeper server associated with
     * the session will be removed. The watches left on those nodes (and on
     * their parents) will be triggered.
     * <p>
     * Added in 3.5.3: <a href="https://docs.oracle.com/javase/tutorial/essential/exceptions/tryResourceClose.html">try-with-resources</a>
     * may be used instead of calling close directly.
     * </p>
     * <p>
     * This method does not wait for all internal threads to exit.
     * Use the {@link #close(int) } method to wait for all resources to be released
     * </p>
     *
     * @throws InterruptedException
     */
    public void close() {
        if (!cnxn.getState().isAlive()) {
            LOG.debug("Close called on already closed client");
            return;
        }

        LOG.debug("Closing session: 0x" + Long.toHexString(getSessionId()));

        try {
            cnxn.close();
        } catch (IOException e) {
            LOG.debug("Ignoring unexpected exception during close", e);
        }

        LOG.info("Session: 0x{} closed", Long.toHexString(getSessionId()));
    }

    /**
     * Close this client object as the {@link #close() } method.
     * This method will wait for internal resources to be released.
     *
     * @param waitForShutdownTimeoutMs timeout (in milliseconds) to wait for resources to be released.
     * Use zero or a negative value to skip the wait
     * @throws InterruptedException
     * @return true if waitForShutdownTimeout is greater than zero and all of the resources have been released
     *
     * @since 3.5.4
     */
    // TODO:
    /*
    public bool close(int waitForShutdownTimeoutMs) {
        close();
        return testableWaitForShutdown(waitForShutdownTimeoutMs);
    }
    */

    /**
     * Prepend the chroot to the client path (if present). The expectation of
     * this function is that the client path has been validated before this
     * function is called
     * @param clientPath path to the node
     * @return server view of the path (chroot prepended to client path)
     */
    internal String prependChroot(String clientPath) {
        return chroot.prepend(clientPath);
    }

    public void Dispose()
    {
        close();
    }
}


public static class ZookeeperStatesExtensions
{
    public static bool isAlive(this ZooKeeper.States states) {
        return states != ZooKeeper.States.CLOSED && states != ZooKeeper.States.AUTH_FAILED;
    }

    /**
     * Returns whether we are connected to a server (which
     * could possibly be read-only, if this client is allowed
     * to go to read-only mode)
     * */
    public static bool isConnected(this ZooKeeper.States states) {
        return states == ZooKeeper.States.CONNECTED || states == ZooKeeper.States.CONNECTEDREADONLY;
    }
}
