namespace ApacheZooKeeper;

using ApacheZooKeeper.JavaPorts;

public class ClientCnxnSocket
{
    public string getLocalSocketAddress() => string.Empty;
    public string getRemoteSocketAddress() => string.Empty;

    public void connectionPrimed()
    {
    }

    public void connect(InetSocketAddress addr)
    {
    }

    internal void introduce(ClientCnxn.SendThread sendThread,
        long clientCnxnSessionId,
        LinkedBlockingDeque<ClientCnxn.Packet> clientCnxnOutgoingQueue)
    {
    }

    public void updateNow()
    {
    }

    public void updateLastSendAndHeard()
    {
    }

    public void updateLastSend()
    {
    }

    public bool isConnected() => false;

    public int getIdleRecv()
    {
        throw new NotImplementedException();
    }

    public int getIdleSend()
    {
        throw new NotImplementedException();
    }

    public void doTransport(int to, Queue<ClientCnxn.Packet> clientCnxnPendingQueue, ClientCnxn clientCnxn)
    {
        throw new NotImplementedException();
    }

    public void close()
    {
        throw new NotImplementedException();
    }

    public void cleanup()
    {
        throw new NotImplementedException();
    }

    public void onClosing()
    {
        throw new NotImplementedException();
    }

    public void testableCloseSocket()
    {
        throw new NotImplementedException();
    }

    public void sendPacket(ClientCnxn.Packet packet)
    {
        throw new NotImplementedException();
    }

    public void saslCompleted()
    {
        throw new NotImplementedException();
    }

    public void packetAdded()
    {
        throw new NotImplementedException();
    }
}
