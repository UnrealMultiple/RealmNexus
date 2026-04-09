// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace RealmNexus.Models;

public class Server
{
    public string Name { get; set; } = string.Empty;
    public string ServerIP { get; set; } = "127.0.0.1";
    public ushort ServerPort { get; set; } = 7777;
}