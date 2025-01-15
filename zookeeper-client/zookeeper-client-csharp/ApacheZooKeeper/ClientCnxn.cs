namespace ApacheZooKeeper;

using ApacheZooKeeper.Client;
using ApacheZooKeeper.Jute;
using ApacheZooKeeper.Logging;
using ApacheZooKeeper.Proto;

using System.Collections.Concurrent;
using System.Text;

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
    private static readonly int SET_WATCHES_MAX_LENGTH = 128 * 1024;

    /* predefined xid's values recognized as special by the server */
    // -1 means notification(WATCHER_EVENT)
    public static readonly int NOTIFICATION_XID = -1;
    // -2 is the xid for pings
    public static readonly int PING_XID = -2;
    // -4 is the xid for AuthPacket
    public static readonly int AUTHPACKET_XID = -4;
    // -8 is the xid for setWatch
    public static readonly int SET_WATCHES_XID = -8;

    public class AuthData {

        AuthData(String scheme, byte[] data) {
            this.scheme = scheme;
            this.data = data;
        }

        String scheme;

        byte[] data;

    }

    private readonly ConcurrentBag<AuthData> authInfo = new();

    /**
     * These are the packets that have been sent and are waiting for a response.
     */
    //TODO:
    //private readonly Queue<Packet> pendingQueue = new();

    /**
     * These are the packets that need to be sent.
     */
    //TODO:
    //private readonly LinkedBlockingDeque<Packet> outgoingQueue = new LinkedBlockingDeque<>();

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

    readonly Task sendThread;

    readonly Task eventThread;

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

        RequestHeader requestHeader;

        ReplyHeader replyHeader;

        IRecord request;

        Record response;

        ByteBuffer bb;

        /** Client's view of the path (may differ due to chroot) **/
        String clientPath;
        /** Servers's view of the path (may differ due to chroot) **/
        String serverPath;

        bool finished;

        AsyncCallback cb;

        Object ctx;

        WatchRegistration watchRegistration;

        WatchDeregistration watchDeregistration;

        /** Convenience ctor */
        Packet(
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
                MemoryStream baos = new MemoryStream();
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
                baos.close();
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

    }

}
