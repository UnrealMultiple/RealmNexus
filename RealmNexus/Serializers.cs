using TrProtocol;

 namespace RealmNexus;

 internal static class Serializers
 {
     public static readonly PacketSerializer ClientSerializer = new(true);
     public static readonly PacketSerializer ServerSerializer = new(false);
 }