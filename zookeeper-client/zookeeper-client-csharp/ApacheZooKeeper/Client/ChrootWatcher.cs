namespace ApacheZooKeeper.Client;

public class ChrootWatcher : Watcher {
    private readonly Chroot.NotRoot chroot;
    private readonly Watcher watcher;

    public ChrootWatcher(Chroot.NotRoot chroot, Watcher watcher) {
        this.chroot = chroot;
        this.watcher = watcher;
    }

    public override void process(WatchedEvent @event) {
        string path = @event.getPath();
        if (path != null) {
            path = chroot.strip(path);
                @event = new WatchedEvent(@event.getType(), @event.getState(), path, @event.getZxid());
        }
        watcher.process(@event);
    }
}
