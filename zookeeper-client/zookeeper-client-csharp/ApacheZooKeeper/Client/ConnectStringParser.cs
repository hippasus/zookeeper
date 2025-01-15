namespace ApacheZooKeeper.Client;

using ApacheZooKeeper.Common;
using ApacheZooKeeper.JavaPorts;

using System.Net;

public sealed class ConnectStringParser
{
    private static readonly int DEFAULT_PORT = 2181;

    private readonly String chrootPath;

    private readonly List<InetSocketAddress> serverAddresses = new();

    /**
     * Parse host and port by splitting client connectString
     * with support for IPv6 literals
     * @throws IllegalArgumentException
     *             for an invalid chroot path.
     */
    public ConnectStringParser(String connectString) {
        // parse out chroot, if any
        int off = connectString.indexOf('/');
        if (off >= 0) {
            String chrootPath = connectString.substring(off);
            // ignore "/" chroot spec, same as null
            if (chrootPath.length() == 1) {
                this.chrootPath = null;
            } else {
                PathUtils.validatePath(chrootPath);
                this.chrootPath = chrootPath;
            }
            connectString = connectString.substring(0, off);
        } else {
            this.chrootPath = null;
        }

        var hostsList = connectString.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (String host in hostsList)
        {
            var parsedHost = host;
            int port = DEFAULT_PORT;
            String[] hostAndPort = NetUtils.getIPV6HostAndPort(host);
            if (hostAndPort.Length != 0) {
                parsedHost = hostAndPort[0];
                if (hostAndPort.Length == 2) {
                    port = Integer.parseInt(hostAndPort[1]);
                }
            } else {
                int pidx = host.lastIndexOf(':');
                if (pidx >= 0) {
                    // otherwise : is at the end of the string, ignore
                    if (pidx < host.length() - 1) {
                        port = Integer.parseInt(host.substring(pidx + 1));
                    }
                    parsedHost = host.substring(0, pidx);
                }
            }

            serverAddresses.Add(new InetSocketAddress { Host = parsedHost, Port = port });
        }
    }

    public String getChrootPath() {
        return chrootPath;
    }

    public List<InetSocketAddress> getServerAddresses() {
        return serverAddresses;
    }
}
