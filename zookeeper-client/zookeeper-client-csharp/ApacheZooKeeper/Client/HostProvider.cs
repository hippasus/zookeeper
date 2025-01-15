namespace ApacheZooKeeper.Client;

using ApacheZooKeeper.JavaPorts;

using System.Collections.ObjectModel;

/// <summary>
/// A set of hosts a ZooKeeper client should connect to.
///
/// Classes implementing this interface must guarantee the following:
///
/// * Every call to next() returns an InetIPEndPoint. So the iterator never
/// ends.
///
/// * The size() of a HostProvider may never be zero.
///
/// A HostProvider must return resolved InetIPEndPoint instances on next() if the next address is resolvable.
/// In that case, it's up to the HostProvider, whether it returns the next resolvable address in the list or return
/// the next one as UnResolved.
///
/// Different HostProvider could be imagined:
///
/// * A HostProvider that loads the list of Hosts from an URL or from DNS
/// * A HostProvider that re-resolves the InetIPEndPoint after a timeout.
/// * A HostProvider that prefers nearby hosts.
///
/// </summary>
public interface HostProvider
{
    int size();

    /// <summary>
    /// The next host to try to connect to.
    /// For a spinDelay of 0 there should be no wait.
    /// </summary>
    /// <param name="spinDelay">Milliseconds to wait if all hosts have been tried once.</param>
    /// <returns>the socket address</returns>
    InetSocketAddress next(long spinDelay);

    /// <summary>
    ///
    /// Notify the HostProvider of a successful connection.
    ///
    /// The HostProvider may use this notification to reset it's inner state.
    ///
    /// </summary>
    void onConnected();

    /// <summary>
    /// Update the list of servers. This returns true if changing connections is necessary for load-balancing, false otherwise.
    /// </summary>
    /// <param name="serverAddresses">new host list</param>
    /// <param name="currentHost">the host to which this client is currently connected</param>
    /// <returns>true if changing connections is necessary for load-balancing, false otherwise</returns>
    bool updateServerList(Collection<InetSocketAddress> serverAddresses, InetSocketAddress currentHost);
}
