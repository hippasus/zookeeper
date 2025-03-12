namespace ApacheZooKeeper;

using ApacheZooKeeper.Client;
using ApacheZooKeeper.Compact;
using ApacheZooKeeper.JavaPorts;
using ApacheZooKeeper.Jute;
using ApacheZooKeeper.Logging;
using ApacheZooKeeper.Proto;

using System.Net;
using System.Text;

public abstract class ClientCnxnSocket
{
    private static readonly ILogger LOG = LoggerManager.GetLogger<ClientCnxnSocket>();

    private readonly ProtocolManager protocolManager = new ProtocolManager();

    protected bool initialized;

    /**
     * This buffer is only used to read the length of the incoming message.
     */
    protected readonly ByteBuffer lenBuffer = ByteBuffer.allocateDirect(4);

    /**
     * After the length is read, a new incomingBuffer is allocated in
     * readLength() to receive the full message.
     */
    protected ByteBuffer incomingBuffer;
    protected readonly AtomicLong sentCount = new AtomicLong(0L);
    protected readonly AtomicLong recvCount = new AtomicLong(0L);
    // Used for reactive timeout detection, say connection read timeout and session expiration timeout.
    protected long lastHeard;
    // Used for proactive timeout detection, say ping timeout and connection establishment timeout.
    protected long lastSend;
    protected long now;
    protected ClientCnxn.SendThread sendThread;
    protected LinkedBlockingDeque<ClientCnxn.Packet> outgoingQueue;
    protected ZKClientConfig clientConfig;
    private int packetLen = 0xfffff; //ZKClientConfig.CLIENT_MAX_PACKET_LENGTH_DEFAULT;

    /**
     * The sessionId is only available here for Log and Exception messages.
     * Otherwise the socket doesn't need to know it.
     */
    protected long sessionId;

    public ClientCnxnSocket()
    {
        incomingBuffer = lenBuffer;
    }

    internal void introduce(ClientCnxn.SendThread sendThread, long sessionId, LinkedBlockingDeque<ClientCnxn.Packet> outgoingQueue) {
        this.sendThread = sendThread;
        this.sessionId = sessionId;
        this.outgoingQueue = outgoingQueue;
    }

    internal void updateNow() {
        now = Time.currentElapsedTime();
    }

    internal int getIdleRecv() {
        return (int) (now - lastHeard);
    }

    internal int getIdleSend() {
        return (int) (now - lastSend);
    }

    internal long getSentCount() {
        return sentCount.get();
    }

    internal long getRecvCount() {
        return recvCount.get();
    }

    internal void updateLastHeard() {
        this.lastHeard = now;
    }

    internal void updateLastSend() {
        this.lastSend = now;
    }

    internal void updateLastSendAndHeard() {
        this.lastSend = now;
        this.lastHeard = now;
    }

    void readLength() {
        int len = incomingBuffer.getInt();
        if (len < 0 || len > packetLen) {
            throw new IOException("Packet len " + len + " is out of range!");
        }
        incomingBuffer = ByteBuffer.allocate(len);
    }

    public void readConnectResult() {
        if (LOG.isTraceEnabled()) {
            StringBuilder buf = new StringBuilder("0x[");
            foreach (byte b in incomingBuffer.array()) {
                buf.append(Integer.toHexString(b)).append(",");
            }
            buf.append("]");
            if (LOG.isTraceEnabled()) {
                LOG.trace("readConnectResult {0} {1}", incomingBuffer.remaining(), buf);
            }
        }

        ByteBufferInputStream bbis = new ByteBufferInputStream(incomingBuffer);
        BinaryInputArchive bbia = BinaryInputArchive.getArchive(bbis);
        ConnectResponse conRsp = protocolManager.deserializeConnectResponse(bbia);
        if (!protocolManager.isReadonlyAvailable()) {
            LOG.warn("Connected to an old server; r-o mode will be unavailable");
        }
        this.sessionId = conRsp.getSessionId();
        sendThread.onConnected(conRsp.getTimeOut(), this.sessionId, conRsp.getPasswd(), conRsp.getReadOnly());
    }

    public abstract bool isConnected();

    public abstract Task connect(InetSocketAddress addr);

    /**
     * Returns the address to which the socket is connected.
     */
    public abstract EndPoint getRemoteSocketAddress();

    /**
     * Returns the address to which the socket is bound.
     */
    public abstract EndPoint getLocalSocketAddress();

    /**
     * Clean up resources for a fresh new socket.
     * It's called before reconnect or close.
     */
    public abstract void cleanup();

    /**
     * new packets are added to outgoingQueue.
     */
    public abstract void packetAdded();

    /**
     * connState is marked CLOSED and notify ClientCnxnSocket to react.
     */
    public abstract void onClosing();

    /**
     * Sasl completes. Allows non-priming packgets to be sent.
     * Note that this method will only be called if Sasl starts and completes.
     */
    public abstract void saslCompleted();

    /**
     * being called after ClientCnxn finish PrimeConnection
     */
    public abstract void connectionPrimed();

    /**
     * Do transportation work:
     * - read packets into incomingBuffer.
     * - write outgoing queue packets.
     * - update relevant timestamp.
     *
     * @param waitTimeOut timeout in blocking wait. Unit in MilliSecond.
     * @param pendingQueue These are the packets that have been sent and
     *                     are waiting for a response.
     * @param cnxn
     * @throws IOException
     * @throws InterruptedException
     */
    public abstract Task doTransport(
        int waitTimeOut,
        Queue<ClientCnxn.Packet> pendingQueue,
        ClientCnxn cnxn);

    /**
     * Close the socket.
     */
    public abstract void testableCloseSocket();

    /**
     * Close this client.
     */
    public abstract void close();

    /**
     * Send Sasl packets directly.
     * The Sasl process will send the first (requestHeader == null) packet,
     * and then block the doTransport write,
     * finally unblock it when finished.
     */
    public abstract void sendPacket(ClientCnxn.Packet p);

    // TODO
    /*
    protected void initProperties() {
        try {
            packetLen = clientConfig.getInt(
                ZKConfig.JUTE_MAXBUFFER,
                ZKClientConfig.CLIENT_MAX_PACKET_LENGTH_DEFAULT);
            LOG.info("{} value is {} Bytes", ZKConfig.JUTE_MAXBUFFER, packetLen);
        } catch (NumberFormatException e) {
            String msg = MessageFormat.format(
                "Configured value {0} for property {1} can not be parsed to int",
                clientConfig.getProperty(ZKConfig.JUTE_MAXBUFFER),
                ZKConfig.JUTE_MAXBUFFER);
            LOG.error(msg);
            throw new IOException(msg);
        }
    }
    */
}
