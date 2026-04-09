using RealmNexus.Core;
using RealmNexus.Models;
using RealmNexus.Packets;

namespace RealmNexus.Handlers;

public class CustomPacketHandler : ClientHandler
{
    public override void OnC2SPacket(PacketReceiveArgs args)
    {
        if (args.Packet is RealmNexusUpdate)
        {
            args.Handled = true;
        }
    }

    public override void OnS2CPacket(PacketReceiveArgs args)
    {
        if (args.Packet is not RealmNexusUpdate update)
        {
            return;
        }

        Logger.Log("DimensionPackets", LogLevel.INFO, $"收到维度数据包: {update}");

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (update.SubType)
        {
            case SubMessageID.OnlineInfoRequest:
                Parent.SendServer(new RealmNexusUpdate
                {
                    SubType = SubMessageID.OnlineInfoResponse,
                    Content = string.Join("\n", GlobalTracker.GetClientNames())
                });
                break;
            case SubMessageID.ChangeSever:
                var server = Program.Config.GetServer(update.Content);

                Parent.ChangeServer(server);
                break;
            case SubMessageID.ChangeCustomizedServer:
                Parent.ChangeServer(new Server
                {
                    Name = "Customized Server",
                    ServerIP = update.Content,
                    ServerPort = update.Port
                });
                break;
        }
        args.Handled = true;
    }
}