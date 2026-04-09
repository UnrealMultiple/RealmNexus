using RealmNexus.Core;

namespace RealmNexus.Handlers;

public static class GlobalTracker
{
    private static readonly HashSet<Client> Clients = [];

    public static void OnClientConnection(Client client)
    {
        lock (Clients)
        {
            Clients.Add(client);
            client.PacketClient.OnError += _ =>
            {
                lock (Clients)
                {
                    Clients.Remove(client);
                }
            };
        }
    }

    public static string[] GetClientNames()
    {
        lock (Clients)
        {
            return Clients.Select(c => c.Name).Where(n => n != null).Distinct().ToArray();
        }
    }
}