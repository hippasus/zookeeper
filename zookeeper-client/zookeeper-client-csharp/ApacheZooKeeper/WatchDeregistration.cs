namespace ApacheZooKeeper;

using EventType = Watcher.Event.EventType;
using WatcherType = Watcher.WatcherType;

public class WatchDeregistration
{
    private readonly String serverPath;
    private readonly Watcher watcher;
    private readonly WatcherType watcherType;
    private readonly bool local;
    private readonly ZKWatchManager zkManager;

    public WatchDeregistration(
        String serverPath,
        Watcher watcher,
        WatcherType watcherType,
        bool local,
        ZKWatchManager zkManager) {
        this.serverPath = serverPath;
        this.watcher = watcher;
        this.watcherType = watcherType;
        this.local = local;
        this.zkManager = zkManager;
    }

    /**
     * Unregistering watcher that was added on path.
     *
     * @param rc
     *            the result code of the operation that attempted to remove
     *            watch on the path.
     */
    public IDictionary<EventType, HashSet<Watcher>> unregister(int rc) {
        //return zkManager.removeWatcher(serverPath, watcher, watcherType, local, rc);
        throw new NotImplementedException();
    }

    /**
     * Returns server path which has specified for unregistering its watcher
     *
     * @return server path
     */
    public String getServerPath() {
        return serverPath;
    }
}
