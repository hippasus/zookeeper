namespace ApacheZooKeeper.Server;

public abstract class ZooKeeperThread : IDisposable
{
    private CancellationTokenSource _cts = new();
    private Task _task;

    public void start()
    {
        _task ??= Task.Run(() => run(_cts.Token));
    }

    protected virtual void run(CancellationToken ct)
    {
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
