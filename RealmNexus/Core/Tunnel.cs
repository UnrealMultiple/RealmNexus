using RealmNexus.Packets;
using TrProtocol;

namespace RealmNexus.Core;

public class Tunnel(PacketClient source, PacketClient target, string prefix)
{
    public event Action<PacketReceiveArgs> OnReceive;
    public event Action<Exception> OnError;
    public event Action OnClose;

    private bool shouldStop;

    public void Start()
    {
        Task.Run(RunTunnel);
    }

    private void RunTunnel()
    {
        try
        {
            while (!shouldStop && source.Client.Connected && target.Client.Connected)
            {
                var packet = source.Receive();
                if (packet == null)
                {
                    continue;
                }

                var args = new PacketReceiveArgs(packet);
                OnReceive?.Invoke(args);
                if (args.Handled)
                {
                    continue;
                }
                //Console.WriteLine($"{prefix} Tunneling: {packet}");

                // Handle both INetPacket and DimensionUpdate
                if (packet is RealmNexusUpdate dimensionUpdate)
                {
                    Logger.Log("Tunnel", LogLevel.DEBUG, $"{prefix} 转发 DimensionUpdate");
                    target.Send(dimensionUpdate);
                }
                else if (packet is INetPacket netPacket)
                {
                    Logger.Log("Tunnel", LogLevel.DEBUG, $"{prefix} 转发 {netPacket.GetType().Name}");
                    target.Send(netPacket);
                }
            }
        }
        catch (Exception e)
        {
            OnError?.Invoke(e);
        }
        finally
        {
            OnClose?.Invoke();
        }
    }

    public void Close()
    {
        shouldStop = true;
    }
}