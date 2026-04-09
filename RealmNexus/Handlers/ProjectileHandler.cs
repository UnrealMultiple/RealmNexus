using RealmNexus.Core;
using TrProtocol.NetPackets;

namespace RealmNexus.Handlers;

public class ProjectileHandler : ClientHandler
{
    private const short maxProjectile = 1000;
    private readonly short[] projOwner = new short[maxProjectile];

    public ProjectileHandler()
    {
        for (short i = 0; i < maxProjectile; ++i)
        {
            projOwner[i] = -1;
        }
    }
    public override void OnCommonPacket(PacketReceiveArgs args)
    {
        switch (args.Packet)
        {
            case SyncProjectile sync:
                projOwner[sync.ProjSlot] = sync.PlayerSlot;
                break;
            case KillProjectile kill:
            {
                if (projOwner[kill.ProjSlot] == kill.PlayerSlot)
                {
                    projOwner[kill.PlayerSlot] = -1;
                }

                break;
            }
        }
    }

    public override void OnCleaning()
    {
        for (short i = 0; i < maxProjectile; ++i)
        {
            if (projOwner[i] == -1)
            {
                continue;
            }

            Parent.SendClient(new KillProjectile
            {
                PlayerSlot = (byte)projOwner[i],
                ProjSlot = i
            });
            projOwner[i] = -1;
        }
    }
}