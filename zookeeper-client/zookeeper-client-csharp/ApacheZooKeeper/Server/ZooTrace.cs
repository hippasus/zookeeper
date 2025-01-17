namespace ApacheZooKeeper.Server;

using ApacheZooKeeper.Logging;
using ApacheZooKeeper.Server.Quorum;

public class ZooTrace
{
    private static readonly object _lockObject = new();

    public static readonly long CLIENT_REQUEST_TRACE_MASK = 1 << 1;

    /**
     * this field is obsolete
     */
    [Obsolete]
    public static readonly long CLIENT_DATA_PACKET_TRACE_MASK = 1 << 2;

    public static readonly long CLIENT_PING_TRACE_MASK = 1 << 3;

    public static readonly long SERVER_PACKET_TRACE_MASK = 1 << 4;

    public static readonly long SESSION_TRACE_MASK = 1 << 5;

    public static readonly long EVENT_DELIVERY_TRACE_MASK = 1 << 6;

    public static readonly long SERVER_PING_TRACE_MASK = 1 << 7;

    public static readonly long WARNING_TRACE_MASK = 1 << 8;

    /**
     * this field is obsolete
     */
    [Obsolete]
    public static readonly long JMX_TRACE_MASK = 1 << 9;

    private static long traceMask = CLIENT_REQUEST_TRACE_MASK | SERVER_PACKET_TRACE_MASK | SESSION_TRACE_MASK | WARNING_TRACE_MASK;

    public static long getTextTraceLevel() {
        lock (_lockObject) {
            return traceMask;
        }
    }

    public static void setTextTraceLevel(long mask) {
        lock (_lockObject) {
            traceMask = mask;
            ILogger LOG = LoggerManager.GetLogger<ZooTrace>();
            LOG.info("Set text trace mask to 0x{}", Long.toHexString(mask));
        }
    }

    public static bool isTraceEnabled(ILogger log, long mask) {
        lock (_lockObject) {
            return log.isTraceEnabled() && (mask & traceMask) != 0;
        }
    }

    public static void logTraceMessage(ILogger log, long mask, String msg) {
        if (isTraceEnabled(log, mask)) {
            log.trace(msg);
        }
    }

    /*
    public static void logQuorumPacket(ILogger log, long mask, char direction, QuorumPacket qp) {
        if (isTraceEnabled(log, mask)) {
            logTraceMessage(log, mask, direction + " " + LearnerHandler.packetToString(qp));
        }
    }

    public static void logRequest(ILogger log, long mask, char rp, Request request, String header) {
        if (isTraceEnabled(log, mask)) {
            log.trace(header + ":" + rp + request.toString());
        }
    }
    */

}
