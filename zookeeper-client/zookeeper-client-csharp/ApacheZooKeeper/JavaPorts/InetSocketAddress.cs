namespace ApacheZooKeeper.JavaPorts;

public record InetSocketAddress
{
    public string Host { get; set; }
    public int Port { get; set; }

    public override string ToString() => $"{Host}:{Port}";
}
