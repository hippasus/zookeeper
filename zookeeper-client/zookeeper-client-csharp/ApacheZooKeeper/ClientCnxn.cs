namespace ApacheZooKeeper;

using ApacheZooKeeper.Client;
using ApacheZooKeeper.Common;
using ApacheZooKeeper.JavaPorts;
using ApacheZooKeeper.Jute;
using ApacheZooKeeper.Logging;
using ApacheZooKeeper.Proto;
using ApacheZooKeeper.Server;

using System.Collections.Concurrent;
using System.Text;

using EventType = Watcher.Event.EventType;
using KeeperState = Watcher.Event.KeeperState;
using OpCode = ZooDefs.OpCode;
using States = ZooKeeper.States;
using WatchRegistration = ZooKeeper.WatchRegistration;

public class ClientCnxn {
    private static readonly ILogger LOG = LoggerManager.GetLogger<ClientCnxn>();

    /* ZOOKEEPER-706: If a session has a large number of watches set then
     * attempting to re-establish those watches after a connection loss may
     * fail due to the SetWatches request exceeding the server's configured
     * jute.maxBuffer value. To avoid this we instead split the watch
     * re-establishement across multiple SetWatches calls. This constant
     * controls the size of each call. It is set to 128kB to be conservative
     * with respect to the server's 1MB default for jute.maxBuffer.
     */
    private const int SET_WATCHES_MAX_LENGTH = 128 * 1024;

    /* predefined xid's values recognized as special by the server */
    // -1 means notification(WATCHER_EVENT)
    public const int NOTIFICATION_XID = -1;
    // -2 is the xid for pings
    public const int PING_XID = -2;
    // -4 is the xid for AuthPacket
    public const int AUTHPACKET_XID = -4;
    // -8 is the xid for setWatch
    public const int SET_WATCHES_XID = -8;

    public class AuthData {

        AuthData(String scheme, byte[] data) {
            this.scheme = scheme;
            this.data = data;
        }

        internal String scheme;

        internal byte[] data;

    }

    private readonly ConcurrentBag<AuthData> authInfo = new();

    /**
     * These are the packets that have been sent and are waiting for a response.
     */
    private readonly Queue<Packet> pendingQueue = new();

    /**
     * These are the packets that need to be sent.
     */
    private readonly LinkedBlockingDeque<Packet> outgoingQueue = new();

    private int connectTimeout;

    /**
     * The timeout in ms the client negotiated with the server. This is the
     * "real" timeout, not the timeout request by the client (which may have
     * been increased/decreased by the server which applies bounds to this
     * value.
     */
    private volatile int negotiatedSessionTimeout;

    private int readTimeout;

    private int expirationTimeout;

    private readonly int sessionTimeout;

    private readonly ZKWatchManager watchManager;

    private long sessionId;

    private byte[] sessionPasswd;

    /**
     * If true, the connection is allowed to go to r-o mode. This field's value
     * is sent, besides other data, during session creation handshake. If the
     * server on the other side of the wire is partitioned it'll accept
     * read-only clients only.
     */
    private bool readOnly;

    readonly SendThread sendThread;

    readonly EventThread eventThread;

    /**
     * Set to true when close is called. Latches the connection such that we
     * don't attempt to re-connect to the server if in the middle of closing the
     * connection (client sends session disconnect to server as part of close
     * operation)
     */
    private volatile bool closing = false;

    /**
     * A set of ZooKeeper hosts this client could connect to.
     */
    private readonly HostProvider hostProvider;

    /**
     * Is set to true when a connection to a r/w server is established for the
     * first time; never changed afterwards.
     * <p>
     * Is used to handle situations when client without sessionId connects to a
     * read-only server. Such client receives "fake" sessionId from read-only
     * server, but this sessionId is invalid for other servers. So when such
     * client finds a r/w server, it sends 0 instead of fake sessionId during
     * connection handshake and establishes new, valid session.
     * <p>
     * If this field is false (which implies we haven't seen r/w server before)
     * then non-zero sessionId is fake, otherwise it is valid.
     */
    volatile bool seenRwServerBefore = false;

    private readonly ZKClientConfig clientConfig;
    /**
     * If any request's response in not received in configured requestTimeout
     * then it is assumed that the response packet is lost.
     */
    private long requestTimeout;


    public ZKWatchManager getWatcherManager() {
     return watchManager;
    }

    public long getSessionId() {
     return sessionId;
    }

    public byte[] getSessionPasswd() {
     return sessionPasswd;
    }

    public int getSessionTimeout() {
     return negotiatedSessionTimeout;
    }

    //TODO:
    /*
    public override String ToString() {
     StringBuilder sb = new StringBuilder();

     IPEndPoint local = sendThread.getClientCnxnSocket().getLocalSocketAddress();
     IPEndPoint remote = sendThread.getClientCnxnSocket().getRemoteSocketAddress();
     sb.append("sessionid:0x").append(Long.toHexString(getSessionId()))
      .append(" local:").append(local)
      .append(" remoteserver:").append(remote)
      .append(" lastZxid:").append(lastZxid)
      .append(" xid:").append(xid)
      .append(" sent:").append(sendThread.getClientCnxnSocket().getSentCount())
      .append(" recv:").append(sendThread.getClientCnxnSocket().getRecvCount())
      .append(" queuedpkts:").append(outgoingQueue.size())
      .append(" pendingresp:").append(pendingQueue.size())
      .append(" queuedevents:").append(eventThread.waitingEvents.size());

     return sb.toString();
    }
    */


    /**
     * This class allows us to pass the headers and the relevant records around.
     */
    public class Packet {

        internal RequestHeader requestHeader;

        internal ReplyHeader replyHeader;

        internal Record request;

        internal Record response;

        internal ByteBuffer bb;

        /** Client's view of the path (may differ due to chroot) **/
        internal String clientPath;
        /** Servers's view of the path (may differ due to chroot) **/
        internal String serverPath;

        internal bool finished;

        internal AsyncCallback cb;

        internal Object ctx;

        internal WatchRegistration watchRegistration;

        internal WatchDeregistration watchDeregistration;

        /** Convenience ctor */
        internal Packet(
            RequestHeader requestHeader,
            ReplyHeader replyHeader,
            Record request,
            Record response,
            WatchRegistration watchRegistration
        ) {
            this.requestHeader = requestHeader;
            this.replyHeader = replyHeader;
            this.request = request;
            this.response = response;
            this.watchRegistration = watchRegistration;
        }

        public void createBB() {
            try {
                using MemoryStream baos = new MemoryStream();
                BinaryOutputArchive boa = BinaryOutputArchive.getArchive(baos);
                boa.writeInt(-1, "len"); // We'll fill this in later
                if (requestHeader != null) {
                    requestHeader.serialize(boa, "header");
                }
                if (request is ConnectRequest) {
                    request.serialize(boa, "connect");
                } else if (request != null) {
                    request.serialize(boa, "request");
                }
                baos.Position = 0;
                this.bb = ByteBuffer.wrap(baos.toByteArray());
                this.bb.putInt(this.bb.capacity() - 4);
                this.bb.rewind();
            } catch (IOException e) {
                LOG.warn("Unexpected exception", e);
            }
        }

        public override String ToString() {
            StringBuilder sb = new StringBuilder();

            sb.append("clientPath:" + clientPath);
            sb.append(" serverPath:" + serverPath);
            sb.append(" finished:" + finished);

            sb.append(" header:: " + requestHeader);
            sb.append(" replyHeader:: " + replyHeader);
            sb.append(" request:: " + request);
            sb.append(" response:: " + response);

            // jute toString is horrible, remove unnecessary newlines
            return sb.toString().replaceAll("\r*\n+", " ");
        }

        private event EventHandler<EventArgs> Finished;

        internal void notifyAll()
        {
            Finished?.Invoke(this, EventArgs.Empty);
        }

    }

    /**
     * Creates a connection object. The actual network connect doesn't get
     * established until needed. The start() instance method must be called
     * after construction.
     *
     * @param hostProvider the list of ZooKeeper servers to connect to
     * @param sessionTimeout the timeout for connections.
     * @param clientConfig the client configuration.
     * @param defaultWatcher default watcher for this connection
     * @param clientCnxnSocket the socket implementation used (e.g. NIO/Netty)
     * @param canBeReadOnly whether the connection is allowed to go to read-only mode in case of partitioning
     */
    public ClientCnxn(
        HostProvider hostProvider,
        int sessionTimeout,
        ZKClientConfig clientConfig,
        Watcher defaultWatcher,
        ClientCnxnSocket clientCnxnSocket,
        bool canBeReadOnly
    )
        : this(
            hostProvider,
            sessionTimeout,
            clientConfig,
            defaultWatcher,
            clientCnxnSocket,
            0,
            new byte[16],
            canBeReadOnly)
    {
    }

    /**
     * Creates a connection object. The actual network connect doesn't get
     * established until needed. The start() instance method must be called
     * after construction.
     *
     * @param hostProvider the list of ZooKeeper servers to connect to
     * @param sessionTimeout the timeout for connections.
     * @param clientConfig the client configuration.
     * @param defaultWatcher default watcher for this connection
     * @param clientCnxnSocket the socket implementation used (e.g. NIO/Netty)
     * @param sessionId session id if re-establishing session
     * @param sessionPasswd session passwd if re-establishing session
     * @param canBeReadOnly whether the connection is allowed to go to read-only mode in case of partitioning
     * @throws IOException in cases of broken network
     */
    public ClientCnxn(
        HostProvider hostProvider,
        int sessionTimeout,
        ZKClientConfig clientConfig,
        Watcher defaultWatcher,
        ClientCnxnSocket clientCnxnSocket,
        long sessionId,
        byte[] sessionPasswd,
        bool canBeReadOnly
    ) {
        this.hostProvider = hostProvider;
        this.sessionTimeout = sessionTimeout;
        this.clientConfig = clientConfig;
        this.sessionId = sessionId;
        this.sessionPasswd = sessionPasswd;
        this.readOnly = canBeReadOnly;

        this.watchManager = new ZKWatchManager(
                false, //clientConfig.getBoolean(ZKClientConfig.DISABLE_AUTO_WATCH_RESET), // TODO:
                defaultWatcher);

        this.connectTimeout = sessionTimeout / hostProvider.size();
        this.readTimeout = sessionTimeout * 2 / 3;
        this.expirationTimeout = sessionTimeout * 4 / 3;

        this.sendThread = new SendThread(this, clientCnxnSocket);
        this.eventThread = new EventThread(this);
        initRequestTimeout();
    }

    public void start() {
        sendThread.start();
        eventThread.start();
    }


    private object eventOfDeath = new object();

    private class WatcherSetEventPair {

        internal readonly HashSet<Watcher> watchers;
        internal readonly WatchedEvent @event;

        public WatcherSetEventPair(HashSet<Watcher> watchers, WatchedEvent @event) {
            this.watchers = watchers;
            this.@event = @event;
        }

    }

    /**
     * Guard against creating "-EventThread-EventThread-EventThread-..." thread
     * names when ZooKeeper object is being created from within a watcher.
     * See ZOOKEEPER-795 for details.
     */
    private static String makeThreadName(String suffix) {
        String name = Thread.CurrentThread.Name.replaceAll("-EventThread", "");
        return name + suffix;
    }

    /**
     * Tests that current thread is the main event loop.
     * This method is useful only for tests inside ZooKeeper project
     * it is not a public API intended for use by external applications.
     * @return true if Thread.currentThread() is an EventThread.
     */
    public static bool isInEventThread() {
        // TODO:
        //return Thread.currentThread() instanceof EventThread;
        throw new NotImplementedException();
    }

    class EventThread : ZooKeeperThread {
        private readonly ClientCnxn _clientCnxn;

        private readonly BlockingCollection<object> waitingEvents = new BlockingCollection<object>();

        /** This is really the queued session state until the event
         * thread actually processes the event and hands it to the watcher.
         * But for all intents and purposes this is the state.
         */
        private volatile Watcher.Event.KeeperState sessionState = Watcher.Event.KeeperState.Disconnected;

        private volatile bool wasKilled = false;
        private volatile bool isRunning = false;

        public EventThread(ClientCnxn clientCnxn)
        {
            _clientCnxn = clientCnxn;
        }

        public void queueEvent(WatchedEvent @event) {
            queueEvent(@event, null);
        }

        internal void queueEvent(WatchedEvent @event, HashSet<Watcher> materializedWatchers) {
            if (@event.getType() == Watcher.Event.EventType.None && sessionState == @event.getState()) {
                return;
            }
            sessionState = @event.getState();
            HashSet<Watcher> watchers;
            if (materializedWatchers == null) {
                // materialize the watchers based on the event
                watchers = _clientCnxn.watchManager.materialize(@event.getState(), @event.getType(), @event.getPath());
            } else {
                watchers = new(materializedWatchers);
            }
            WatcherSetEventPair pair = new WatcherSetEventPair(watchers, @event);
            // queue the pair (watch set & event) for later processing
            waitingEvents.add(pair);
        }

        public void queueCallback(AsyncCallback cb, int rc, String path, Object ctx) {
            waitingEvents.add(new LocalCallback(cb, rc, path, ctx));
        }

        public void queuePacket(Packet packet) {
            if (wasKilled) {
                lock (waitingEvents) {
                    if (isRunning) {
                        waitingEvents.add(packet);
                    } else {
                        processEvent(packet);
                    }
                }
            } else {
                waitingEvents.add(packet);
            }
        }

        public void queueEventOfDeath() {
            waitingEvents.add(_clientCnxn.eventOfDeath);
        }

        protected override void run(CancellationToken ct) {
            try {
                isRunning = true;
                while (!ct.IsCancellationRequested) {
                    Object @event = waitingEvents.take();
                    if (@event == _clientCnxn.eventOfDeath) {
                        wasKilled = true;
                    } else {
                        processEvent(@event);
                    }
                    if (wasKilled) {
                        lock (waitingEvents) {
                            if (waitingEvents.isEmpty()) {
                                isRunning = false;
                                break;
                            }
                        }
                    }
                }
            } catch (ThreadInterruptedException e) {
                LOG.error("Event thread exiting due to interruption", e);
            }

            LOG.info("EventThread shut down for session: 0x{}", Long.toHexString(_clientCnxn.getSessionId()));
        }

        private void processEvent(Object @event) {
            try {
                if (@event is WatcherSetEventPair) {
                    // each watcher will process the event
                    WatcherSetEventPair pair = (WatcherSetEventPair)@event;
                    foreach (Watcher watcher in pair.watchers) {
                        try {
                            watcher.process(pair.@event);
                        } catch (Exception t) {
                            LOG.error("Error while calling watcher.", t);
                        }
                    }
                } else if (@event is LocalCallback lcb) {
                    if (lcb.cb is StatCallback) {
                        ((StatCallback) lcb.cb).processResult(lcb.rc, lcb.path, lcb.ctx, null);
                    } else if (lcb.cb is DataCallback) {
                        ((DataCallback) lcb.cb).processResult(lcb.rc, lcb.path, lcb.ctx, null, null);
                    } else if (lcb.cb is ACLCallback) {
                        ((ACLCallback) lcb.cb).processResult(lcb.rc, lcb.path, lcb.ctx, null, null);
                    } else if (lcb.cb is ChildrenCallback) {
                        ((ChildrenCallback) lcb.cb).processResult(lcb.rc, lcb.path, lcb.ctx, null);
                    } else if (lcb.cb is Children2Callback) {
                        ((Children2Callback) lcb.cb).processResult(lcb.rc, lcb.path, lcb.ctx, null, null);
                    } else if (lcb.cb is StringCallback) {
                        ((StringCallback) lcb.cb).processResult(lcb.rc, lcb.path, lcb.ctx, null);
                    } else if (lcb.cb is EphemeralsCallback) {
                        ((EphemeralsCallback) lcb.cb).processResult(lcb.rc, lcb.ctx, null);
                    } else if (lcb.cb is AllChildrenNumberCallback) {
                        ((AllChildrenNumberCallback) lcb.cb).processResult(lcb.rc, lcb.path, lcb.ctx, -1);
                    } else if (lcb.cb is MultiCallback) {
                        ((MultiCallback) lcb.cb).processResult(lcb.rc, lcb.path, lcb.ctx, Collections.emptyList<OpResult>());
                    } else {
                        ((VoidCallback) lcb.cb).processResult(lcb.rc, lcb.path, lcb.ctx);
                    }
                } else {
                    Packet p = (Packet) @event;
                    int rc = 0;
                    String clientPath = p.clientPath;
                    if (p.replyHeader.getErr() != 0) {
                        rc = p.replyHeader.getErr();
                    }
                    if (p.cb == null) {
                        LOG.warn("Somehow a null cb got to EventThread!");
                    } else if (p.response is ExistsResponse
                               || p.response is SetDataResponse
                               || p.response is SetACLResponse) {
                        StatCallback cb = (StatCallback) p.cb;
                        if (rc == KeeperException.Code.OK.intValue()) {
                            if (p.response is ExistsResponse) {
                                cb.processResult(rc, clientPath, p.ctx, ((ExistsResponse) p.response).getStat());
                            } else if (p.response is SetDataResponse) {
                                cb.processResult(rc, clientPath, p.ctx, ((SetDataResponse) p.response).getStat());
                            } else if (p.response is SetACLResponse) {
                                cb.processResult(rc, clientPath, p.ctx, ((SetACLResponse) p.response).getStat());
                            }
                        } else {
                            cb.processResult(rc, clientPath, p.ctx, null);
                        }
                    } else if (p.response is GetDataResponse) {
                        DataCallback cb = (DataCallback) p.cb;
                        GetDataResponse rsp = (GetDataResponse) p.response;
                        if (rc == KeeperException.Code.OK.intValue()) {
                            cb.processResult(rc, clientPath, p.ctx, rsp.getData(), rsp.getStat());
                        } else {
                            cb.processResult(rc, clientPath, p.ctx, null, null);
                        }
                    } else if (p.response is GetACLResponse) {
                        ACLCallback cb = (ACLCallback) p.cb;
                        GetACLResponse rsp = (GetACLResponse) p.response;
                        if (rc == KeeperException.Code.OK.intValue()) {
                            cb.processResult(rc, clientPath, p.ctx, rsp.getAcl(), rsp.getStat());
                        } else {
                            cb.processResult(rc, clientPath, p.ctx, null, null);
                        }
                    } else if (p.response is GetChildrenResponse) {
                        ChildrenCallback cb = (ChildrenCallback) p.cb;
                        GetChildrenResponse rsp = (GetChildrenResponse) p.response;
                        if (rc == KeeperException.Code.OK.intValue()) {
                            cb.processResult(rc, clientPath, p.ctx, rsp.getChildren());
                        } else {
                            cb.processResult(rc, clientPath, p.ctx, null);
                        }
                    } else if (p.response is GetAllChildrenNumberResponse) {
                        AllChildrenNumberCallback cb = (AllChildrenNumberCallback) p.cb;
                        GetAllChildrenNumberResponse rsp = (GetAllChildrenNumberResponse) p.response;
                        if (rc == KeeperException.Code.OK.intValue()) {
                            cb.processResult(rc, clientPath, p.ctx, rsp.getTotalNumber());
                        } else {
                            cb.processResult(rc, clientPath, p.ctx, -1);
                        }
                    } else if (p.response is GetChildren2Response) {
                        Children2Callback cb = (Children2Callback) p.cb;
                        GetChildren2Response rsp = (GetChildren2Response) p.response;
                        if (rc == KeeperException.Code.OK.intValue()) {
                            cb.processResult(rc, clientPath, p.ctx, rsp.getChildren(), rsp.getStat());
                        } else {
                            cb.processResult(rc, clientPath, p.ctx, null, null);
                        }
                    } else if (p.response is CreateResponse) {
                        StringCallback cb = (StringCallback) p.cb;
                        CreateResponse rsp = (CreateResponse) p.response;
                        if (rc == KeeperException.Code.OK.intValue()) {
                            cb.processResult(
                                rc,
                                clientPath,
                                p.ctx,
                                rsp.getPath());
                        } else {
                            cb.processResult(rc, clientPath, p.ctx, null);
                        }
                    } else if (p.response is Create2Response) {
                        Create2Callback cb = (Create2Callback) p.cb;
                        Create2Response rsp = (Create2Response) p.response;
                        if (rc == KeeperException.Code.OK.intValue()) {
                            cb.processResult(
                                    rc,
                                    clientPath,
                                    p.ctx,
                                    rsp.getPath(),
                                    rsp.getStat());
                        } else {
                            cb.processResult(rc, clientPath, p.ctx, null, null);
                        }
                    } else if (p.response is MultiResponse) {
                        MultiCallback cb = (MultiCallback) p.cb;
                        MultiResponse rsp = (MultiResponse) p.response;
                        if (rc == KeeperException.Code.OK.intValue()) {
                            List<OpResult> results = rsp.getResultList();
                            int newRc = rc;
                            foreach (OpResult result in results) {
                                if (result is OpResult.ErrorResult
                                    && KeeperException.Code.OK.intValue()
                                       != (newRc = ((OpResult.ErrorResult) result).getErr())) {
                                    break;
                                }
                            }
                            cb.processResult(newRc, clientPath, p.ctx, results);
                        } else {
                            cb.processResult(rc, clientPath, p.ctx, null);
                        }
                    } else if (p.response is GetEphemeralsResponse) {
                        EphemeralsCallback cb = (EphemeralsCallback) p.cb;
                        GetEphemeralsResponse rsp = (GetEphemeralsResponse) p.response;
                        if (rc == KeeperException.Code.OK.intValue()) {
                            cb.processResult(rc, p.ctx, rsp.getEphemerals());
                        } else {
                            cb.processResult(rc, p.ctx, null);
                        }
                    } else if (p.cb is VoidCallback) {
                        VoidCallback cb = (VoidCallback) p.cb;
                        cb.processResult(rc, clientPath, p.ctx);
                    }
                }
            } catch (Exception t) {
                LOG.error("Unexpected throwable", t);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                waitingEvents.Dispose();
            }
        }
    }

    // @VisibleForTesting
    protected void finishPacket(Packet p) {
        int err = p.replyHeader.getErr();
        if (p.watchRegistration != null) {
            p.watchRegistration.register(err);
        }
        // Add all the removed watch events to the event queue, so that the
        // clients will be notified with 'Data/Child WatchRemoved' event type.
        if (p.watchDeregistration != null) {
            Dictionary<EventType, HashSet<Watcher>> materializedWatchers = null;
            try {
                materializedWatchers = p.watchDeregistration.unregister(err);
                foreach (KeyValuePair<EventType, HashSet<Watcher>> entry in materializedWatchers.entrySet()) {
                    HashSet<Watcher> watchers = entry.getValue();
                    if (watchers.size() > 0) {
                        queueEvent(p.watchDeregistration.getServerPath(), err, watchers, entry.getKey());
                        // ignore connectionloss when removing from local
                        // session
                        p.replyHeader.setErr(KeeperException.Code.OK.intValue());
                    }
                }
            } catch (KeeperException.NoWatcherException nwe) {
                p.replyHeader.setErr(nwe.code().intValue());
            } catch (KeeperException ke) {
                p.replyHeader.setErr(ke.code().intValue());
            }
        }

        if (p.cb == null) {
            lock (p) {
                p.finished = true;
                p.notifyAll();
            }
        } else {
            p.finished = true;
            eventThread.queuePacket(p);
        }
    }

    void queueEvent(String serverPath, int err, HashSet<Watcher> materializedWatchers, EventType eventType) {
        KeeperState sessionState = KeeperState.SyncConnected;
        if (KeeperException.Code.SESSIONEXPIRED.intValue() == err
            || KeeperException.Code.CONNECTIONLOSS.intValue() == err) {
            sessionState = KeeperState.Disconnected;
        }
        WatchedEvent @event = new WatchedEvent(eventType, sessionState, serverPath);
        eventThread.queueEvent(@event, materializedWatchers);
    }

    void queueCallback(AsyncCallback cb, int rc, String path, Object ctx) {
        eventThread.queueCallback(cb, rc, path, ctx);
    }


    // for test only
    protected void onConnecting(InetSocketAddress addr) {

    }

    private void conLossPacket(Packet p) {
        if (p.replyHeader == null) {
            return;
        }
        switch (state) {
        case AUTH_FAILED:
            p.replyHeader.setErr(KeeperException.Code.AUTHFAILED.intValue());
            break;
        case CLOSED:
            p.replyHeader.setErr(KeeperException.Code.SESSIONEXPIRED.intValue());
            break;
        default:
            p.replyHeader.setErr(KeeperException.Code.CONNECTIONLOSS.intValue());
        }
        finishPacket(p);
    }

    private VolatileLong lastZxid = new(0L);

    public long getLastZxid() {
        return lastZxid.Value;
    }

    public class EndOfStreamException : IOException {

        private static readonly long serialVersionUID = -5438877188796231422L;

        public EndOfStreamException(String msg)
            : base (msg)
        {
        }

        public override String ToString() {
            return "EndOfStreamException: " + Message;
        }

    }

    private class ConnectionTimeoutException : IOException {
        public ConnectionTimeoutException(String message)
            : base(message)
        {
        }
    }

    private class SessionTimeoutException : IOException {

        private static readonly long serialVersionUID = 824482094072071178L;

        public SessionTimeoutException(String msg)
            : base(msg)
        {
        }

    }

    private class SessionExpiredException : IOException {

        private static readonly long serialVersionUID = -1388816932076193249L;

        public SessionExpiredException(String msg)
            : base(msg)
        {
        }

    }

    private class RWServerFoundException : IOException {

        private static readonly long serialVersionUID = 90431199887158758L;

        public RWServerFoundException(String msg)
            : base(msg)
        {
        }

    }


    /**
     * This class services the outgoing request queue and generates the heart
     * beats. It also spawns the ReadThread.
     */
    class SendThread : ZooKeeperThread {
        private ClientCnxn _clientCnxn;

        private long lastPingSentNs;
        private readonly ClientCnxnSocket clientCnxnSocket;
        private bool isFirstConnect = true;
        private volatile ZooKeeperSaslClient zooKeeperSaslClient;
        private readonly AtomicReference<Login> loginRef = new(null);

        void readResponse(ByteBuffer incomingBuffer) {
            ByteBufferInputStream bbis = new ByteBufferInputStream(incomingBuffer);
            BinaryInputArchive bbia = BinaryInputArchive.getArchive(bbis);
            ReplyHeader replyHdr = new ReplyHeader();

            replyHdr.deserialize(bbia, "header");
            switch (replyHdr.getXid()) {
            case PING_XID:
                LOG.debug("Got ping response for session id: 0x{} after {}ms.",
                    Long.toHexString(_clientCnxn.sessionId),
                    ((TimeExtensions.nanoTime() - lastPingSentNs) / 1000000));
                return;
              case AUTHPACKET_XID:
                LOG.debug("Got auth session id: 0x{}", Long.toHexString(_clientCnxn.sessionId));
                if (replyHdr.getErr() == KeeperException.Code.AUTHFAILED.intValue()) {
                    changeZkState(States.AUTH_FAILED);
                    _clientCnxn.eventThread.queueEvent(new WatchedEvent(Watcher.Event.EventType.None,
                        Watcher.Event.KeeperState.AuthFailed, null));
                    _clientCnxn.eventThread.queueEventOfDeath();
                }
              return;
            case NOTIFICATION_XID:
                LOG.debug("Got notification session id: 0x{}",
                    Long.toHexString(_clientCnxn.sessionId));
                WatcherEvent @event = new WatcherEvent();
                @event.deserialize(bbia, "response");

                WatchedEvent we = new WatchedEvent(@event, replyHdr.getZxid());
                LOG.debug("Got {} for session id 0x{}", we, Long.toHexString(_clientCnxn.sessionId));
                _clientCnxn.eventThread.queueEvent(we);
                return;
            default:
                break;
            }

            // If SASL authentication is currently in progress, construct and
            // send a response packet immediately, rather than queuing a
            // response as with other packets.
            if (tunnelAuthInProgress()) {
                GetSASLRequest request = new GetSASLRequest();
                request.deserialize(bbia, "token");
                zooKeeperSaslClient.respondToServer(request.getToken(), _clientCnxn);
                return;
            }

            Packet packet;
            lock (_clientCnxn.pendingQueue) {
                if (_clientCnxn.pendingQueue.size() == 0) {
                    throw new IOException("Nothing in the queue, but got " + replyHdr.getXid());
                }
                packet = _clientCnxn.pendingQueue.remove();
            }
            /*
             * Since requests are processed in order, we better get a response
             * to the first request!
             */
            try {
                if (packet.requestHeader.getXid() != replyHdr.getXid()) {
                    packet.replyHeader.setErr(KeeperException.Code.CONNECTIONLOSS.intValue());
                    throw new IOException("Xid out of order. Got Xid " + replyHdr.getXid()
                                          + " with err " + replyHdr.getErr()
                                          + " expected Xid " + packet.requestHeader.getXid()
                                          + " for a packet with details: " + packet);
                }

                packet.replyHeader.setXid(replyHdr.getXid());
                packet.replyHeader.setErr(replyHdr.getErr());
                packet.replyHeader.setZxid(replyHdr.getZxid());
                if (replyHdr.getZxid() > 0) {
                    _clientCnxn.lastZxid.Value = replyHdr.getZxid();
                }
                if (packet.response != null && replyHdr.getErr() == 0) {
                    packet.response.deserialize(bbia, "response");
                }

                LOG.debug("Reading reply session id: 0x{}, packet:: {}", Long.toHexString(_clientCnxn.sessionId), packet);
            } finally {
                _clientCnxn.finishPacket(packet);
            }
        }

        SendThread(ClientCnxn clientCnxn, ClientCnxnSocket clientCnxnSocket) {
            _clientCnxn = clientCnxn;
            changeZkState(States.CONNECTING);
            this.clientCnxnSocket = clientCnxnSocket;
        }

        // TODO: can not name this method getState since Thread.getState()
        // already exists
        // It would be cleaner to make class SendThread an implementation of
        // Runnable
        /**
         * Used by ClientCnxnSocket
         *
         * @return
         */
        ZooKeeper.States getZkState() {
            return _clientCnxn.state;
        }

        void changeZkState(ZooKeeper.States newState) {
            if (!_clientCnxn.state.isAlive() && newState == States.CONNECTING) {
                throw new IOException(
                        "Connection has already been closed and reconnection is not allowed");
            }
            // It's safer to place state modification at the end.
            _clientCnxn.state = newState;
        }

        ClientCnxnSocket getClientCnxnSocket() {
            return clientCnxnSocket;
        }

        /**
         * Setup session, previous watches, authentication.
         */
        void primeConnection() {
            LOG.info(
                "Socket connection established, initiating session, client: {}, server: {}",
                clientCnxnSocket.getLocalSocketAddress(),
                clientCnxnSocket.getRemoteSocketAddress());
            isFirstConnect = false;
            long sessId = (_clientCnxn.seenRwServerBefore) ? _clientCnxn.sessionId : 0;
            ConnectRequest conReq = new ConnectRequest(0, _clientCnxn.lastZxid.Value, _clientCnxn.sessionTimeout, sessId, _clientCnxn.sessionPasswd, _clientCnxn.readOnly);
            // We add backwards since we are pushing into the front
            // Only send if there's a pending watch
            if (!_clientCnxn.clientConfig.getBoolean(ZKClientConfig.DISABLE_AUTO_WATCH_RESET)) {
                var watchManager = _clientCnxn.watchManager;
                List<String> dataWatches = watchManager.getDataWatchList();
                List<String> existWatches = watchManager.getExistWatchList();
                List<String> childWatches = watchManager.getChildWatchList();
                List<String> persistentWatches = watchManager.getPersistentWatchList();
                List<String> persistentRecursiveWatches = watchManager.getPersistentRecursiveWatchList();
                if (!dataWatches.isEmpty() || !existWatches.isEmpty() || !childWatches.isEmpty()
                        || !persistentWatches.isEmpty() || !persistentRecursiveWatches.isEmpty()) {
                    using var dataWatchesIter = dataWatches.iterator();
                    using var existWatchesIter = existWatches.iterator();
                    using var childWatchesIter = childWatches.iterator();
                    using var persistentWatchesIter = persistentWatches.iterator();
                    using var persistentRecursiveWatchesIter = persistentRecursiveWatches.iterator();
                    long setWatchesLastZxid = _clientCnxn.lastZxid.Value;

                    while (dataWatchesIter.hasNext() || existWatchesIter.hasNext() || childWatchesIter.hasNext()
                            || persistentWatchesIter.hasNext() || persistentRecursiveWatchesIter.hasNext())
                    {
                        List<String> dataWatchesBatch = new();
                        List<String> existWatchesBatch = new();
                        List<String> childWatchesBatch = new();
                        List<String> persistentWatchesBatch = new();
                        List<String> persistentRecursiveWatchesBatch = new();
                        int batchLength = 0;

                        // Note, we may exceed our max length by a bit when we add the last
                        // watch in the batch. This isn't ideal, but it makes the code simpler.
                        while (batchLength < SET_WATCHES_MAX_LENGTH) {
                            String watch;
                            if (dataWatchesIter.hasNext()) {
                                watch = dataWatchesIter.next();
                                dataWatchesBatch.add(watch);
                            } else if (existWatchesIter.hasNext()) {
                                watch = existWatchesIter.next();
                                existWatchesBatch.add(watch);
                            } else if (childWatchesIter.hasNext()) {
                                watch = childWatchesIter.next();
                                childWatchesBatch.add(watch);
                            }  else if (persistentWatchesIter.hasNext()) {
                                watch = persistentWatchesIter.next();
                                persistentWatchesBatch.add(watch);
                            } else if (persistentRecursiveWatchesIter.hasNext()) {
                                watch = persistentRecursiveWatchesIter.next();
                                persistentRecursiveWatchesBatch.add(watch);
                            } else {
                                break;
                            }
                            batchLength += watch.length();
                        }

                        Record record;
                        OpCode opcode;
                        if (persistentWatchesBatch.isEmpty() && persistentRecursiveWatchesBatch.isEmpty()) {
                            // maintain compatibility with older servers - if no persistent/recursive watchers
                            // are used, use the old version of SetWatches
                            record = new SetWatches(setWatchesLastZxid, dataWatchesBatch, existWatchesBatch, childWatchesBatch);
                            opcode = OpCode.setWatches;
                        } else {
                            record = new SetWatches2(setWatchesLastZxid, dataWatchesBatch, existWatchesBatch,
                                    childWatchesBatch, persistentWatchesBatch, persistentRecursiveWatchesBatch);
                            opcode = OpCode.setWatches2;
                        }
                        RequestHeader header = new RequestHeader(ClientCnxn.SET_WATCHES_XID, (int)opcode);
                        Packet packet = new Packet(header, new ReplyHeader(), record, null, null);
                        _clientCnxn.outgoingQueue.addFirst(packet);
                    }
                }
            }

            foreach (AuthData id in authInfo) {
                _clientCnxn.outgoingQueue.addFirst(
                    new Packet(
                        new RequestHeader(ClientCnxn.AUTHPACKET_XID, (int)OpCode.auth),
                        null,
                        new AuthPacket(0, id.scheme, id.data),
                        null,
                        null));
            }
            _clientCnxn.outgoingQueue.addFirst(new Packet(null, null, conReq, null, null));
            clientCnxnSocket.connectionPrimed();
            LOG.debug("Session establishment request sent on {0}", clientCnxnSocket.getRemoteSocketAddress());
        }

        private void sendPing() {
            lastPingSentNs = TimeExtensions.nanoTime();
            RequestHeader h = new RequestHeader(ClientCnxn.PING_XID, (int)OpCode.ping);
            _clientCnxn.queuePacket(h, null, null, null, null, null, null, null, null);
        }

        private InetSocketAddress rwServerAddress = null;

        private static readonly int minPingRwTimeout = 100;

        private static readonly int maxPingRwTimeout = 60000;

        private int pingRwTimeout = minPingRwTimeout;

        // Set to true if and only if constructor of ZooKeeperSaslClient
        // throws a LoginException: see startConnect() below.
        private bool saslLoginFailed = false;

        private void startConnect(InetSocketAddress addr) {
            // initializing it for new connection
            saslLoginFailed = false;
            if (!isFirstConnect) {
                try {
                    Thread.Sleep(ThreadLocalRandom.current().nextLong(1000));
                } catch (ThreadInterruptedException e) {
                    LOG.warn("Unexpected exception", e);
                }
            }
            changeZkState(States.CONNECTING);

            String hostPort = addr.getHostString() + ":" + addr.getPort();
            MDC.put("myid", hostPort);
            setName(getName().replaceAll("\\(.*\\)", "(" + hostPort + ")"));
            if (clientConfig.isSaslClientEnabled()) {
                try {
                    zooKeeperSaslClient = new ZooKeeperSaslClient(
                        SaslServerPrincipal.getServerPrincipal(addr, clientConfig), clientConfig, loginRef);
                } catch (LoginException e) {
                    // An authentication error occurred when the SASL client tried to initialize:
                    // for Kerberos this means that the client failed to authenticate with the KDC.
                    // This is different from an authentication error that occurs during communication
                    // with the Zookeeper server, which is handled below.
                    LOG.warn(
                        "SASL configuration failed. "
                            + "Will continue connection to Zookeeper server without "
                            + "SASL authentication, if Zookeeper server allows it.", e);
                    eventThread.queueEvent(new WatchedEvent(Watcher.Event.EventType.None, Watcher.Event.KeeperState.AuthFailed, null));
                    saslLoginFailed = true;
                }
            }
            logStartConnect(addr);

            clientCnxnSocket.connect(addr);
        }

        private void logStartConnect(InetSocketAddress addr) {
            LOG.info("Opening socket connection to server {}.", addr);
            if (zooKeeperSaslClient != null) {
                LOG.info("SASL config status: {}", zooKeeperSaslClient.getConfigStatus());
            }
        }

        protected override void run(CancellationToken ct) {
            clientCnxnSocket.introduce(this, sessionId, outgoingQueue);
            clientCnxnSocket.updateNow();
            clientCnxnSocket.updateLastSendAndHeard();
            int to;
            long lastPingRwServer = Time.currentElapsedTime();
            int MAX_SEND_PING_INTERVAL = 10000; //10 seconds
            InetSocketAddress serverAddress = null;
            while (state.isAlive()) {
                try {
                    if (!clientCnxnSocket.isConnected()) {
                        // don't re-establish connection if we are closing
                        if (closing) {
                            break;
                        }
                        if (rwServerAddress != null) {
                            serverAddress = rwServerAddress;
                            rwServerAddress = null;
                        } else {
                            serverAddress = hostProvider.next(1000);
                        }
                        onConnecting(serverAddress);
                        startConnect(serverAddress);
                        // Update now to start the connection timer right after we make a connection attempt
                        clientCnxnSocket.updateNow();
                        clientCnxnSocket.updateLastSend();
                    }

                    if (state.isConnected()) {
                        // determine whether we need to send an AuthFailed event.
                        if (zooKeeperSaslClient != null) {
                            boolean sendAuthEvent = false;
                            if (zooKeeperSaslClient.getSaslState() == ZooKeeperSaslClient.SaslState.INITIAL) {
                                try {
                                    zooKeeperSaslClient.initialize(ClientCnxn.this);
                                } catch (SaslException e) {
                                    LOG.error("SASL authentication with Zookeeper Quorum member failed.", e);
                                    changeZkState(States.AUTH_FAILED);
                                    sendAuthEvent = true;
                                }
                            }
                            KeeperState authState = zooKeeperSaslClient.getKeeperState();
                            if (authState != null) {
                                if (authState == KeeperState.AuthFailed) {
                                    // An authentication error occurred during authentication with the Zookeeper Server.
                                    changeZkState(States.AUTH_FAILED);
                                    sendAuthEvent = true;
                                } else {
                                    if (authState == KeeperState.SaslAuthenticated) {
                                        sendAuthEvent = true;
                                    }
                                }
                            }

                            if (sendAuthEvent) {
                                eventThread.queueEvent(new WatchedEvent(Watcher.Event.EventType.None, authState, null));
                                if (state == States.AUTH_FAILED) {
                                    eventThread.queueEventOfDeath();
                                }
                            }
                        }
                        to = readTimeout - clientCnxnSocket.getIdleRecv();
                    } else {
                        to = connectTimeout - clientCnxnSocket.getIdleSend();
                    }

                    int expiration = expirationTimeout - clientCnxnSocket.getIdleRecv();
                    if (expiration <= 0) {
                        String warnInfo = String.format(
                            "Client session timed out, have not heard from server in %dms for session id 0x%s",
                            clientCnxnSocket.getIdleRecv(),
                            Long.toHexString(sessionId));
                        LOG.warn(warnInfo);
                        changeZkState(States.CLOSED);
                        throw new SessionTimeoutException(warnInfo);
                    } else if (to <= 0) {
                        String warnInfo = String.format(
                            "Client connection timed out, have not heard from server in %dms for session id 0x%s",
                            clientCnxnSocket.getIdleRecv(),
                            Long.toHexString(sessionId));
                        throw new ConnectionTimeoutException(warnInfo);
                    }
                    if (state.isConnected()) {
                        //1000(1 second) is to prevent race condition missing to send the second ping
                        //also make sure not to send too many pings when readTimeout is small
                        int timeToNextPing = readTimeout / 2
                                             - clientCnxnSocket.getIdleSend()
                                             - ((clientCnxnSocket.getIdleSend() > 1000) ? 1000 : 0);
                        //send a ping request either time is due or no packet sent out within MAX_SEND_PING_INTERVAL
                        if (timeToNextPing <= 0 || clientCnxnSocket.getIdleSend() > MAX_SEND_PING_INTERVAL) {
                            sendPing();
                            clientCnxnSocket.updateLastSend();
                        } else {
                            if (timeToNextPing < to) {
                                to = timeToNextPing;
                            }
                        }
                    }

                    // If we are in read-only mode, seek for read/write server
                    if (state == States.CONNECTEDREADONLY) {
                        long now = Time.currentElapsedTime();
                        int idlePingRwServer = (int) (now - lastPingRwServer);
                        if (idlePingRwServer >= pingRwTimeout) {
                            lastPingRwServer = now;
                            idlePingRwServer = 0;
                            pingRwTimeout = Math.min(2 * pingRwTimeout, maxPingRwTimeout);
                            pingRwServer();
                        }
                        to = Math.min(to, pingRwTimeout - idlePingRwServer);
                    }

                    clientCnxnSocket.doTransport(to, pendingQueue, ClientCnxn.this);
                } catch (Throwable e) {
                    if (closing) {
                        // closing so this is expected
                        if (LOG.isDebugEnabled()) {
                            LOG.debug(
                                "An exception was thrown while closing send thread for session 0x{}.",
                                Long.toHexString(getSessionId()), e);
                        }
                        break;
                    } else {
                        LOG.warn(
                            "Session 0x{} for server {}, Closing socket connection. "
                                + "Attempting reconnect except it is a SessionExpiredException or SessionTimeoutException.",
                            Long.toHexString(getSessionId()),
                            serverAddress,
                            e);

                        // At this point, there might still be new packets appended to outgoingQueue.
                        // they will be handled in next connection or cleared up if closed.
                        cleanAndNotifyState();
                    }
                }
            }

            lock (_clientCnxn.outgoingQueue) {
                // When it comes to this point, it guarantees that later queued
                // packet to outgoingQueue will be notified of death.
                cleanup();
            }
            clientCnxnSocket.close();
            if (state.isAlive()) {
                eventThread.queueEvent(new WatchedEvent(Event.EventType.None, Event.KeeperState.Disconnected, null));
            }
            if (closing) {
                eventThread.queueEvent(new WatchedEvent(Event.EventType.None, KeeperState.Closed, null));
            } else if (state == States.CLOSED) {
                eventThread.queueEvent(new WatchedEvent(Event.EventType.None, KeeperState.Expired, null));
            }
            eventThread.queueEventOfDeath();

            Login l = loginRef.getAndSet(null);
            if (l != null) {
                l.shutdown();
            }
            ZooTrace.logTraceMessage(
                LOG,
                ZooTrace.getTextTraceLevel(),
                "SendThread exited loop for session: 0x" + Long.toHexString(getSessionId()));
        }

        private void cleanAndNotifyState() {
            cleanup();
            if (state.isAlive()) {
                eventThread.queueEvent(new WatchedEvent(Event.EventType.None, Event.KeeperState.Disconnected, null));
            }
            clientCnxnSocket.updateNow();
        }

        private void pingRwServer() {
            String result = null;
            InetSocketAddress addr = hostProvider.next(0);

            LOG.info("Checking server {} for being r/w. Timeout {}", addr, pingRwTimeout);
            try {
                result = FourLetterWordMain.send4LetterWord(addr.getHostString(), addr.getPort(), "isro", clientConfig, 1000);
            } catch (ConnectException e) {
                // ignore, this just means server is not up
            } catch (IOException | X509Exception.SSLContextException e) {
                // some unexpected error, warn about it
                LOG.warn("Exception while seeking for r/w server.", e);
            }

            if ("rw\n".equals(result)) {
                pingRwTimeout = minPingRwTimeout;
                // save the found address so that it's used during the next
                // connection attempt
                rwServerAddress = addr;
                throw new RWServerFoundException("Majority server found at "
                                                 + addr.getHostString() + ":" + addr.getPort());
            }
        }

        private void cleanup() {
            clientCnxnSocket.cleanup();
            lock (pendingQueue) {
                for (Packet p : pendingQueue) {
                    conLossPacket(p);
                }
                pendingQueue.clear();
            }
            // We can't call outgoingQueue.clear() here because
            // between iterating and clear up there might be new
            // packets added in queuePacket().
            Iterator<Packet> iter = _clientCnxn.outgoingQueue.iterator();
            while (iter.hasNext()) {
                Packet p = iter.next();
                conLossPacket(p);
                iter.remove();
            }
        }

        /**
         * Callback invoked by the ClientCnxnSocket once a connection has been
         * established.
         */
        void onConnected(
            int _negotiatedSessionTimeout,
            long _sessionId,
            byte[] _sessionPasswd,
            bool isRO) {
            _clientCnxn.negotiatedSessionTimeout = _negotiatedSessionTimeout;
            if (_clientCnxn.negotiatedSessionTimeout <= 0) {
                changeZkState(States.CLOSED);

                _clientCnxn.eventThread.queueEvent(new WatchedEvent(Watcher.Event.EventType.None, Watcher.Event.KeeperState.Expired, null));
                _clientCnxn.eventThread.queueEventOfDeath();

                String warnInfo = String.Format(
                    "Unable to reconnect to ZooKeeper service, session {0} has expired",
                    Long.toHexString(_clientCnxn.sessionId));
                LOG.warn(warnInfo);
                throw new SessionExpiredException(warnInfo);
            }

            if (!_clientCnxn.readOnly && isRO) {
                LOG.error("Read/write client got connected to read-only server");
            }

            _clientCnxn.readTimeout = _clientCnxn.negotiatedSessionTimeout * 2 / 3;
            _clientCnxn.expirationTimeout = _clientCnxn.negotiatedSessionTimeout * 4 / 3;
            _clientCnxn.connectTimeout = _clientCnxn.negotiatedSessionTimeout / _clientCnxn.hostProvider.size();
            _clientCnxn.hostProvider.onConnected();
            _clientCnxn.sessionId = _sessionId;
            _clientCnxn.sessionPasswd = _sessionPasswd;
            changeZkState((isRO) ? States.CONNECTEDREADONLY : States.CONNECTED);
            _clientCnxn.seenRwServerBefore |= !isRO;
            LOG.info(
                "Session establishment complete on server {0}, session id = {1}, negotiated timeout = {2}{3}",
                clientCnxnSocket.getRemoteSocketAddress(),
                Long.toHexString(_clientCnxn.sessionId),
                _clientCnxn.negotiatedSessionTimeout,
                (isRO ? " (READ-ONLY mode)" : ""));
            KeeperState eventState = (isRO) ? KeeperState.ConnectedReadOnly : KeeperState.SyncConnected;
            _clientCnxn.eventThread.queueEvent(new WatchedEvent(Watcher.Event.EventType.None, eventState, null));
        }

        void close() {
            try {
                changeZkState(States.CLOSED);
            } catch (IOException e) {
                LOG.warn("Connection close fails when migrates state from {} to CLOSED",
                        getZkState());
            }
            clientCnxnSocket.onClosing();
        }

        void testableCloseSocket() {
            clientCnxnSocket.testableCloseSocket();
        }

        public bool tunnelAuthInProgress() {
            // 1. SASL client is disabled.
            if (!_clientCnxn.clientConfig.isSaslClientEnabled()) {
                return false;
            }

            // 2. SASL login failed.
            if (saslLoginFailed) {
                return false;
            }

            // 3. SendThread has not created the authenticating object yet,
            // therefore authentication is (at the earliest stage of being) in progress.
            if (zooKeeperSaslClient == null) {
                return true;
            }

            // 4. authenticating object exists, so ask it for its progress.
            return zooKeeperSaslClient.clientTunneledAuthenticationInProgress();
        }

        public void sendPacket(Packet p) {
            clientCnxnSocket.sendPacket(p);
        }

        public ZooKeeperSaslClient getZooKeeperSaslClient() {
            return zooKeeperSaslClient;
        }

        // VisibleForTesting
        Login getLogin() {
            return loginRef.get();
        }
    }

}
