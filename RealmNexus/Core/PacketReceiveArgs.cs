namespace RealmNexus.Core;

public class PacketReceiveArgs(object packet)
{
    public readonly object Packet = packet;
    public bool Handled;
}