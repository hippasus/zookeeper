namespace ApacheZooKeeper;

using ApacheZooKeeper.Logging;
using ApacheZooKeeper.Server.Watch;

public class ZKWatchManager
{
    private static readonly ILogger LOG = LoggerManager.GetLogger<ZKWatchManager>();

    private readonly Dictionary<String, HashSet<Watcher>> dataWatches = new();
    private readonly Dictionary<String, HashSet<Watcher>> existWatches = new();
    private readonly Dictionary<String, HashSet<Watcher>> childWatches = new();
    private readonly Dictionary<String, HashSet<Watcher>> persistentWatches = new();
    private readonly Dictionary<String, HashSet<Watcher>> persistentRecursiveWatches = new();
    private readonly bool disableAutoWatchReset;

    private volatile Watcher defaultWatcher;

    public ZKWatchManager(bool disableAutoWatchReset, Watcher defaultWatcher) {
        this.disableAutoWatchReset = disableAutoWatchReset;
        this.defaultWatcher = defaultWatcher;
    }

    public void setDefaultWatcher(Watcher defaultWatcher) {
        this.defaultWatcher = defaultWatcher;
    }

    public Watcher getDefaultWatcher() {
        return defaultWatcher;
    }

    public List<String> getDataWatchList() {
        lock (dataWatches) {
            return dataWatches.keyList();
        }
    }

    public List<String> getChildWatchList() {
        lock (childWatches) {
            return childWatches.keyList();
        }
    }

    public List<String> getExistWatchList() {
        lock (existWatches) {
            return existWatches.keyList();
        }
    }

    public List<String> getPersistentWatchList() {
        lock (persistentWatches) {
            return persistentWatches.keyList();
        }
    }

    public List<String> getPersistentRecursiveWatchList() {
        lock (persistentRecursiveWatches) {
            return persistentRecursiveWatches.keyList();
        }
    }

    public Dictionary<String, HashSet<Watcher>> getDataWatches() {
        return dataWatches;
    }

    public Dictionary<String, HashSet<Watcher>> getExistWatches() {
        return existWatches;
    }

    public Dictionary<String, HashSet<Watcher>> getChildWatches() {
        return childWatches;
    }

    public Dictionary<String, HashSet<Watcher>> getPersistentWatches() {
        return persistentWatches;
    }

    public Dictionary<String, HashSet<Watcher>> getPersistentRecursiveWatches() {
        return persistentRecursiveWatches;
    }

    private void addTo(HashSet<Watcher> from, HashSet<Watcher> to) {
        if (from != null) {
            to.addAll(from);
        }
    }

    public Dictionary<Watcher.Event.EventType, HashSet<Watcher>> removeWatcher(
        String clientPath,
        Watcher watcher,
        Watcher.WatcherType watcherType,
        bool local,
        int rc
    ) {
        // Validate the provided znode path contains the given watcher of
        // watcherType
        containsWatcher(clientPath, watcher, watcherType);

        Dictionary<Watcher.Event.EventType, HashSet<Watcher>> removedWatchers = new();
        HashSet<Watcher> childWatchersToRem = new();
        removedWatchers.put(Watcher.Event.EventType.ChildWatchRemoved, childWatchersToRem);
        HashSet<Watcher> dataWatchersToRem = new();
        removedWatchers.put(Watcher.Event.EventType.DataWatchRemoved, dataWatchersToRem);
        HashSet<Watcher> persistentWatchersToRem = new();
        removedWatchers.put(Watcher.Event.EventType.PersistentWatchRemoved, persistentWatchersToRem);
        bool removedWatcher = false;
        switch (watcherType) {
        case Watcher.WatcherType.Children: {
            lock (childWatches) {
                removedWatcher = removeWatches(childWatches, watcher, clientPath, local, rc, childWatchersToRem);
            }
            break;
        }
        case Watcher.WatcherType.Data: {
            lock (dataWatches) {
                removedWatcher = removeWatches(dataWatches, watcher, clientPath, local, rc, dataWatchersToRem);
            }

            lock (existWatches) {
                bool removedDataWatcher = removeWatches(existWatches, watcher, clientPath, local, rc, dataWatchersToRem);
                removedWatcher |= removedDataWatcher;
            }
            break;
        }
        case Watcher.WatcherType.Persistent: {
            lock (persistentWatches) {
                removedWatcher = removeWatches(persistentWatches, watcher, clientPath, local, rc, persistentWatchersToRem);
            }
            break;
        }
        case Watcher.WatcherType.PersistentRecursive: {
            lock (persistentRecursiveWatches) {
                removedWatcher = removeWatches(persistentRecursiveWatches, watcher, clientPath, local, rc, persistentWatchersToRem);
            }
            break;
        }
        case Watcher.WatcherType.Any: {
            lock (childWatches) {
                removedWatcher = removeWatches(childWatches, watcher, clientPath, local, rc, childWatchersToRem);
            }

            lock (dataWatches) {
                bool removedDataWatcher = removeWatches(dataWatches, watcher, clientPath, local, rc, dataWatchersToRem);
                removedWatcher |= removedDataWatcher;
            }

            lock (existWatches) {
                bool removedDataWatcher = removeWatches(existWatches, watcher, clientPath, local, rc, dataWatchersToRem);
                removedWatcher |= removedDataWatcher;
            }

            lock (persistentWatches) {
                bool removedPersistentWatcher = removeWatches(persistentWatches,
                        watcher, clientPath, local, rc, persistentWatchersToRem);
                removedWatcher |= removedPersistentWatcher;
            }

            lock (persistentRecursiveWatches) {
                bool removedPersistentRecursiveWatcher = removeWatches(persistentRecursiveWatches,
                        watcher, clientPath, local, rc, persistentWatchersToRem);
                removedWatcher |= removedPersistentRecursiveWatcher;
            }

            break;
        }
        }
        // Watcher function doesn't exists for the specified params
        if (!removedWatcher) {
            throw new KeeperException.NoWatcherException(clientPath);
        }
        return removedWatchers;
    }

    private bool contains(String path, Watcher watcherObj, Dictionary<String, HashSet<Watcher>> pathVsWatchers) {
        bool watcherExists = true;
        if (pathVsWatchers == null || pathVsWatchers.size() == 0) {
            watcherExists = false;
        } else {
            HashSet<Watcher> watchers = pathVsWatchers.get(path);
            if (watchers == null) {
                watcherExists = false;
            } else if (watcherObj == null) {
                watcherExists = watchers.size() > 0;
            } else {
                watcherExists = watchers.contains(watcherObj);
            }
        }
        return watcherExists;
    }

    /**
     * Validate the provided znode path contains the given watcher and
     * watcherType
     *
     * @param path
     *            - client path
     * @param watcher
     *            - watcher object reference
     * @param watcherType
     *            - type of the watcher
     * @throws KeeperException.NoWatcherException
     */
    void containsWatcher(String path, Watcher watcher, Watcher.WatcherType watcherType) {
        bool containsWatcher = false;
        switch (watcherType) {
        case Watcher.WatcherType.Children: {
            lock (childWatches) {
                containsWatcher = contains(path, watcher, childWatches);
            }
            break;
        }
        case Watcher.WatcherType.Data: {
            lock (dataWatches) {
                containsWatcher = contains(path, watcher, dataWatches);
            }

            lock (existWatches) {
                bool contains_temp = contains(path, watcher, existWatches);
                containsWatcher |= contains_temp;
            }
            break;
        }
        case Watcher.WatcherType.Persistent: {
            lock (persistentWatches) {
                containsWatcher |= contains(path, watcher, persistentWatches);
            }
            break;
        }
        case Watcher.WatcherType.PersistentRecursive: {
            lock (persistentRecursiveWatches) {
                containsWatcher |= contains(path, watcher, persistentRecursiveWatches);
            }
            break;
        }
        case Watcher.WatcherType.Any: {
            lock (childWatches) {
                containsWatcher = contains(path, watcher, childWatches);
            }

            lock (dataWatches) {
                bool contains_temp = contains(path, watcher, dataWatches);
                containsWatcher |= contains_temp;
            }

            lock (existWatches) {
                bool contains_temp = contains(path, watcher, existWatches);
                containsWatcher |= contains_temp;
            }

            lock (persistentWatches) {
                bool contains_temp = contains(path, watcher,
                        persistentWatches);
                containsWatcher |= contains_temp;
            }

            lock (persistentRecursiveWatches) {
                bool contains_temp = contains(path, watcher,
                        persistentRecursiveWatches);
                containsWatcher |= contains_temp;
            }

            break;
        }
        }
        // Watcher function doesn't exists for the specified params
        if (!containsWatcher) {
            throw new KeeperException.NoWatcherException(path);
        }
    }

    protected bool removeWatches(
        Dictionary<String, HashSet<Watcher>> pathVsWatcher,
        Watcher watcher,
        String path,
        bool local,
        int rc,
        HashSet<Watcher> removedWatchers) {
        if (!local && rc != KeeperException.Code.OK.intValue()) {
            throw KeeperException.create(KeeperExceptionCodeExtensions.get(rc), path);
        }
        bool success = false;
        // When local flag is true, remove watchers for the given path
        // irrespective of rc. Otherwise shouldn't remove watchers locally
        // when sees failure from server.
        if (rc == KeeperException.Code.OK.intValue() || (local && rc != KeeperException.Code.OK.intValue())) {
            // Remove all the watchers for the given path
            if (watcher == null) {
                HashSet<Watcher> pathWatchers = pathVsWatcher.remove(path);
                if (pathWatchers != null) {
                    // found path watchers
                    removedWatchers.addAll(pathWatchers);
                    success = true;
                }
            } else {
                HashSet<Watcher> watchers = pathVsWatcher.get(path);
                if (watchers != null) {
                    if (watchers.remove(watcher)) {
                        // found path watcher
                        removedWatchers.add(watcher);
                        // cleanup <path vs watchlist>
                        if (watchers.size() <= 0) {
                            pathVsWatcher.remove(path);
                        }
                        success = true;
                    }
                }
            }
        }
        return success;
    }

    /* (non-Javadoc)
     * @see org.apache.zookeeper.ClientWatchManager#materialize(Event.KeeperState,
     *                                                        Event.EventType, java.lang.String)
     */
    public HashSet<Watcher> materialize(
        Watcher.Event.KeeperState state,
        Watcher.Event.EventType type,
        String clientPath
    ) {
        HashSet<Watcher> result = new();

        switch (type) {
        case Watcher.Event.EventType.None:
            if (defaultWatcher != null) {
                result.add(defaultWatcher);
            }

            bool clear = disableAutoWatchReset && state != Watcher.Event.KeeperState.SyncConnected;
            lock (dataWatches) {
                foreach (HashSet<Watcher> ws in dataWatches.values()) {
                    result.addAll(ws);
                }
                if (clear) {
                    dataWatches.clear();
                }
            }

            lock (existWatches) {
                foreach (HashSet<Watcher> ws in existWatches.values()) {
                    result.addAll(ws);
                }
                if (clear) {
                    existWatches.clear();
                }
            }

            lock (childWatches) {
                foreach (HashSet<Watcher> ws in childWatches.values()) {
                    result.addAll(ws);
                }
                if (clear) {
                    childWatches.clear();
                }
            }

            lock (persistentWatches) {
                foreach (HashSet<Watcher> ws in persistentWatches.values()) {
                    result.addAll(ws);
                }
            }

            lock (persistentRecursiveWatches) {
                foreach (HashSet<Watcher> ws in persistentRecursiveWatches.values()) {
                    result.addAll(ws);
                }
            }

            return result;
        case Watcher.Event.EventType.NodeDataChanged:
        case Watcher.Event.EventType.NodeCreated:
            lock (dataWatches) {
                addTo(dataWatches.remove(clientPath), result);
            }
            lock (existWatches) {
                addTo(existWatches.remove(clientPath), result);
            }
            addPersistentWatches(clientPath, type, result);
            break;
        case Watcher.Event.EventType.NodeChildrenChanged:
            lock (childWatches) {
                addTo(childWatches.remove(clientPath), result);
            }
            addPersistentWatches(clientPath, type, result);
            break;
        case Watcher.Event.EventType.NodeDeleted:
            lock (dataWatches) {
                addTo(dataWatches.remove(clientPath), result);
            }
            // TODO This shouldn't be needed, but just in case
            lock (existWatches) {
                HashSet<Watcher> list = existWatches.remove(clientPath);
                if (list != null) {
                    addTo(list, result);
                    LOG.warn("We are triggering an exists watch for delete! Shouldn't happen!");
                }
            }
            lock (childWatches) {
                addTo(childWatches.remove(clientPath), result);
            }
            addPersistentWatches(clientPath, type, result);
            break;
        default:
            String errorMsg = string.Format(
                "Unhandled watch event type {0} with state {1} on path {2}",
                type,
                state,
                clientPath);
            LOG.error(errorMsg);
            throw new InvalidOperationException(errorMsg);
        }

        return result;
    }

    private void addPersistentWatches(String clientPath, Watcher.Event.EventType type, HashSet<Watcher> result) {
        lock (persistentWatches) {
            addTo(persistentWatches.get(clientPath), result);
        }
        // The semantics of persistent recursive watch promise no child events on descendant nodes. When there
        // are standard child watches on descendants of node being watched in persistent recursive mode, server
        // will deliver child events to client inevitably. So we have to filter out child events for persistent
        // recursive watches on client side.
        if (type == Watcher.Event.EventType.NodeChildrenChanged) {
            return;
        }
        lock (persistentRecursiveWatches) {
            foreach (String path in PathParentIterator.forAll(clientPath).asIterable()) {
                addTo(persistentRecursiveWatches.get(path), result);
            }
        }
    }
}
