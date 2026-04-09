using RealmNexus.Core;
using TrProtocol.NetPackets.Mobile;

namespace RealmNexus.Handlers;

public class MobileDebugHandler : ClientHandler
{
    public override void OnC2SPacket(PacketReceiveArgs args)
    {
        if (args.Packet is PlayerPlatformInfo packet)
        {
            Logger.Log("Client", LogLevel.DEBUG, $"收到PE数据包 (平台: {packet.PlatformId}, 索引: {packet.PlayerSlot})");
        }
    }
}