using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace RealmNexus.Models;

public class Config
{
    private const string ConfigPath = "config.json";

    private static readonly JsonSerializerSettings SerializerSettings = new () { ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() }, Formatting = Formatting.Indented, Converters = [new StringEnumConverter()] };

    public ushort ListenPort { get; set; } = 7654;
    public bool SendDimensionPacket { get; set; }
    public string ProtocolVersion { get; set; } = "Terraria319";

    public Server[] Servers { get; set; } =
    [
        new () { Name = "ExampleServer", ServerIP = "127.0.0.1", ServerPort = 7777 }
    ];

    [JsonIgnore]
    private Dictionary<string, Server> _serverCache;

    public Server GetServer(string name)
    {
        _serverCache ??= Servers.ToDictionary(s => s.Name!, s => s);
        return _serverCache.GetValueOrDefault(name);
    }

    public static Config Load()
    {
        if (File.Exists(ConfigPath))
        {
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath), SerializerSettings)!;
        }

        var defaults = new Config();
        defaults.Save();
        Logger.Log("Config", LogLevel.INFO, $"已自动生成默认配置文件: {ConfigPath}");
        return defaults;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public void Save()
    {
        File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this, SerializerSettings));
    }
}