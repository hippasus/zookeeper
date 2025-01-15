namespace ApacheZooKeeper.Client;

using ApacheZooKeeper.JavaPorts;

using System.Collections.ObjectModel;

public class StaticHostProvider : HostProvider
{
    public StaticHostProvider(List<InetSocketAddress> serverAddresses)
    {
        throw new NotImplementedException();
    }

    public int size()
    {
        throw new NotImplementedException();
    }

    public InetSocketAddress next(long spinDelay)
    {
        throw new NotImplementedException();
    }

    public void onConnected()
    {
        throw new NotImplementedException();
    }

    public bool updateServerList(Collection<InetSocketAddress> serverAddresses, InetSocketAddress currentHost)
    {
        throw new NotImplementedException();
    }
}
