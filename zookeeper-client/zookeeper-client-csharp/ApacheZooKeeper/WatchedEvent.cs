namespace ApacheZooKeeper;

using ApacheZooKeeper.Proto;

using EventType = Watcher.EventType;
using KeeperState = Watcher.KeeperState;

public class WatchedEvent
{
    public static readonly long NO_ZXID = -1L;

    private readonly KeeperState keeperState;
    private readonly EventType eventType;
    private readonly String path;
    private readonly long zxid;

    /**
     * Create a WatchedEvent with specified type, state, path and zxid
     */
    public WatchedEvent(EventType eventType, KeeperState keeperState, String path, long zxid)
    {
        this.keeperState = keeperState;
        this.eventType = eventType;
        this.path = path;
        this.zxid = zxid;
    }

    /**
     * Create a WatchedEvent with specified type, state and path
     */
    public WatchedEvent(EventType eventType, KeeperState keeperState, String path)
        : this(eventType, keeperState, path, NO_ZXID)
    {
    }

    /**
     * Convert a WatcherEvent sent over the wire into a full-fledged WatchedEvent
     */
    public WatchedEvent(WatcherEvent eventMessage, long zxid)
    {
        keeperState = KeeperStateExtensions.fromInt(eventMessage.getState());
        eventType = EventTypeExtensions.fromInt(eventMessage.getType());
        path = eventMessage.getPath();
        this.zxid = zxid;
    }

    public KeeperState getState()
    {
        return keeperState;
    }

    public EventType getType()
    {
        return eventType;
    }

    public String getPath()
    {
        return path;
    }

    /**
     * Returns the zxid of the transaction that triggered this watch if it is
     * of one of the following types:<ul>
     *   <li>{@link EventType#NodeCreated}</li>
     *   <li>{@link EventType#NodeDeleted}</li>
     *   <li>{@link EventType#NodeDataChanged}</li>
     *   <li>{@link EventType#NodeChildrenChanged}</li>
     * </ul>
     * Otherwise, returns {@value #NO_ZXID}. Note that {@value #NO_ZXID} is also
     * returned by old servers that do not support this feature.
     */
    public long getZxid()
    {
        return zxid;
    }

    public override string ToString()
    {
        return "WatchedEvent state:" + keeperState + " type:" + eventType + " path:" + path + " zxid: " + zxid;
    }

    /**
     *  Convert WatchedEvent to type that can be sent over network
     */
    public WatcherEvent getWrapper()
    {
        return new WatcherEvent(eventType.getIntValue(), keeperState.getIntValue(), path);
    }
}
