using RealmNexus.Core;
using TrProtocol.NetPackets.Modules;

namespace RealmNexus.Handlers;

public class CommandHandler : ClientHandler
{
    public override void OnC2SPacket(PacketReceiveArgs args)
    {
        if (args.Packet is not NetTextModule text)
        {
            return;
        }

        var textC2S = text.TextC2S;

        if (textC2S?.Text != null && textC2S.Text.StartsWith("/server"))
        {
            var arg = textC2S.Text[7..].Trim();
            if (arg.Length == 0 || arg.Equals("list", StringComparison.CurrentCultureIgnoreCase))
            {
                Parent.SendChatMessage("服务器列表:");
                foreach (var server in Program.Config.Servers)
                {
                    Parent.SendChatMessage($"/server {server.Name}");
                }
            }
            else
            {
                var target = Program.Config.GetServer(arg);
                if (target == null)
                {
                    Parent.SendChatMessage($"没有找到名为'{arg}'的服务器!");
                    args.Handled = true;
                    return;
                }
                Parent.ChangeServer(target);
            }
            //handled raw player command
            args.Handled = true;
        }
    }
}