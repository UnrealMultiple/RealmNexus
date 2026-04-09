using System.Collections.Concurrent;
using System.Net.Sockets;
using RealmNexus.Packets;
using TrProtocol;

namespace RealmNexus.Core;

public class PacketClient
{
    private readonly BlockingCollection<object> packets = new();

    public event Action<Exception> OnError = exception =>
    {
        Logger.Log("PacketClient", LogLevel.ERROR, $"发生错误: {exception}");
    };

    private readonly BinaryReader br;
    private readonly BinaryWriter bw;

    public readonly TcpClient Client;

    private readonly PacketSerializer serializer;
    // ReSharper disable once NotAccessedField.Local
    private readonly bool isClientSide;

    public PacketClient(TcpClient client, bool isClient)
    {
        this.Client = client;
        isClientSide = isClient;
        var stream = client.GetStream();
        br = new BinaryReader(stream);
        bw = new BinaryWriter(stream);

        serializer = isClient ? Serializers.ServerSerializer : Serializers.ClientSerializer;
    }

    public void Start()
    {
        Task.Run(ListenThread);
    }

    public void Cancel()
    {
        packets.Add(null!);
    }

    public void Clear()
    {
        while (packets.TryTake(out _))
        {
        }
    }

    public object Receive()
    {
        var b = packets.Take();
        return b;
    }

    public void Send(INetPacket data)
    {
        // Handle ISideSpecific packets - set IsServerSide based on serializer direction
        if (data is TrProtocol.Interfaces.ISideSpecific sideSpecific)
        {
            sideSpecific.IsServerSide = !serializer.Client;
        }
        lock (bw)
        {
            bw.Write(serializer.Serialize(data));
        }
    }

    public unsafe void Send(RealmNexusUpdate data)
    {
        using var ms = new MemoryStream();
        using var bw2 = new BinaryWriter(ms);

        bw2.Write((ushort)0);

        var tempBuffer = new byte[ushort.MaxValue];
        fixed (byte* pTemp = tempBuffer)
        {
            void* ptr = pTemp;
            *(byte*)ptr = (byte)MessageID.Unused67;
            ptr = (byte*)ptr + 1;
            data.WriteContent(ref ptr);
            var contentLen = (int)((byte*)ptr - pTemp);

            ms.Position = 0;
            bw2.Write((ushort)(contentLen + 2));
            bw2.Write(tempBuffer, 0, contentLen);
        }

        lock (bw)
        {
            bw.Write(ms.ToArray());
        }
    }

    private unsafe void ListenThread()
    {
        try
        {
            for (; ; )
            {
                var totalLength = br.ReadUInt16();
                var payloadLen = totalLength - 2;
                if (payloadLen < 0)
                {
                    throw new Exception("Invalid packet length");
                }

                var payload = br.ReadBytes(payloadLen);

                object packet;
                var packetId = payload[0];

                switch (packetId)
                {
                    case (byte)MessageID.Unused67:
                        fixed (byte* pPayload = payload)
                        {
                            void* ptr = pPayload + 1;
                            var end = pPayload + payloadLen;
                            var dimensionUpdate = new RealmNexusUpdate();
                            dimensionUpdate.ReadContent(ref ptr, end);
                            packet = dimensionUpdate;
                        }
                        break;
                    default:
                    {
                        using var ms = new MemoryStream(payload.Length + 2);
                        using var bw2 = new BinaryWriter(ms);
                        bw2.Write((ushort)(payload.Length + 2));
                        bw2.Write(payload);
                        ms.Position = 0;
                        using var br2 = new BinaryReader(ms);
                        packet = serializer.Deserialize(br2);
                    }
                        break;
                }
                packets.Add(packet);
            }
        }
        catch (Exception e)
        {
            OnError?.Invoke(e);
        }
    }
}