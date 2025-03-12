namespace ApacheZooKeeper.Jute;

using ApacheZooKeeper.Proto;

public static class JuteExtensions
{
    public static void setType(this RequestHeader h, ZooDefs.OpCode opcode) => h.setType((int)opcode);
}
