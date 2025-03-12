namespace ApacheZooKeeper.Client;

using ApacheZooKeeper.Data;

public class ChrootCreateCallback : StringCallback, Create2Callback
{
    private Chroot.NotRoot chroot;
    private AsyncCallback callback;

    public ChrootCreateCallback(Chroot.NotRoot chroot, StringCallback callback) {
        this.chroot = chroot;
        this.callback = callback;
    }

    public ChrootCreateCallback(Chroot.NotRoot chroot, Create2Callback callback) {
        this.chroot = chroot;
        this.callback = callback;
    }

    public void processResult(int rc, String path, Object ctx, String name) {
        StringCallback cb = (StringCallback) callback;
        cb.processResult(rc, path, ctx, name == null ? null : chroot.strip(name));
    }

    public void processResult(int rc, String path, Object ctx, String name, Stat stat) {
        Create2Callback cb = (Create2Callback) callback;
        cb.processResult(rc, path, ctx, name == null ? null : chroot.strip(name), stat);
    }
}
