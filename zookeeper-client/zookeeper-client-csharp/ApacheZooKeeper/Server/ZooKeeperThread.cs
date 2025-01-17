namespace ApacheZooKeeper.Server;

public abstract class ZooKeeperThread : IDisposable
{
    private string _name = string.Empty;

    private CancellationTokenSource _cts = new();
    private Task _task;

    public string getName() => _name;
    public void setName(string name) => _name = name;

    public void start()
    {
        _task ??= Task.Run(() => run(_cts.Token));
    }

    protected virtual void run(CancellationToken ct)
    {
    }

    public void close()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public void join()
    {
    }

    public void cleanAndNotifyState()
    {
        // TODO:
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            close();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
