using System.Net;
using RealmNexus.Models;

namespace RealmNexus;

public static class Program
{
    public static readonly Config Config;

    static Program()
    {
        Config = Config.Load();
        Logger.Log("Config", LogLevel.INFO, $"协议版本号: {Config.ProtocolVersion}");
        Logger.Log("Config", LogLevel.INFO, $"侦听端口: {Config.ListenPort}");
        Logger.Log("Config", LogLevel.INFO, $"远程服务器: {(Config.Servers.Length == 0 ? "没有任何服务器配置捏~" : string.Join(',', Config.Servers.Select(x => x.Name)))}");
    }

    public static void Main(string[] args)
    {
        var ipEndPoint = new IPEndPoint(IPAddress.Any, Config.ListenPort);
        var listener = new Listener(ipEndPoint);
        Logger.Log("TcpListener", LogLevel.INFO, $"正在侦听: {ipEndPoint}");
        listener.ListenThread();
    }
}