namespace ApacheZooKeeper;

public enum AddWatchMode
{
    /// <summary>
    /// Set a watcher on the given path that does not get removed when triggered (i.e. it stays active
    /// until it is removed). This watcher is triggered for both data and child events. To remove the watcher, use
    /// <c>RemoveWatches()</c> with <c>WatcherType.Any</c>. The watcher behaves as if you placed an Exists() watch and
    /// a GetData() watch on the ZNode at the given path.
    /// </summary>
    PERSISTENT = ZooDefs.AddWatchModes.persistent,

    /// <summary>
    /// Set a watcher on the given path that: a) does not get removed when triggered (i.e. it stays active
    /// until it is removed); b) applies not only to the registered path but all child paths recursively. This watcher
    /// is triggered for both data and child events. To remove the watcher, use
    /// <c>RemoveWatches()</c> with <c>WatcherType.Any</c>.
    ///
    /// The watcher behaves as if you placed an Exists() watch and
    /// a GetData() watch on the ZNode at the given path <strong>and</strong> any ZNodes that are children
    /// of the given path including children added later.
    ///
    /// NOTE: when there are active recursive watches there is a small performance decrease as all segments
    /// of ZNode paths must be checked for watch triggering.
    /// </summary>
    PERSISTENT_RECURSIVE = ZooDefs.AddWatchModes.persistentRecursive
}

public static class AddWatchModeExtensions
{
    public static int getMode(this AddWatchMode mode) => (int)mode;
}
