# RealmNexus

基于TrProtocol的Terraria代理服务器，[chi-rei-den/Dimensions](https://github.com/chi-rei-den/TrProtocol)的分支

## 功能特性

- 多服务器支持: 支持配置多个Terraria服务器，玩家可以通过命令切换
- 无缝切换: 在不同服务器之间切换时保持玩家状态
- 自定义数据包: 实现了`RealmNexusUpdate`自定义数据包用于服务器间通信
- 实体同步清理: 在切换服务器时清理 NPC、投射物、物品等实体

## 快速开始

### 1. 配置

创建 `config.json` 文件：

```json
{
  "listen_port": 7654,
  "send_dimension_packet": false,
  "protocol_version": "Terraria319",
  "servers": [
    {
      "name": "main",
      "server_ip": "127.0.0.1",
      "server_port": 7777
    },
    {
      "name": "pvp",
      "server_ip": "127.0.0.1",
      "server_port": 7778
    }
  ]
}
```

### 2. 运行

```bash
dotnet run
```

### 3. 连接

在游戏中连接 `localhost:7654` (或配置的监听端口)

## 命令

| 命令               | 描述        |
| ---------------- | --------- |
| `/server`        | 显示可用服务器列表 |
| `/server <name>` | 切换到指定服务器  |
| `/server list`   | 显示可用服务器列表 |

## 架构说明

### 数据包流转

```shell
客户端 <-> PacketClient <-> Tunnel <-> PacketClient <-> 服务端
```

### 核心组件

#### Client

管理单个客户端连接，包含：

- 客户端连接 (`_client`)
- 服务端连接 (`_serverConnection`)
- 数据包隧道 (`c2s`, `s2c`)
- 处理器列表 (`handlers`)

#### Tunnel

负责在两个`PacketClient`之间转发数据包：

- C2S: 客户端到服务端
- S2C: 服务端到客户端

#### PacketClient

封装`TcpClient`，提供数据包级别的读写：

- 使用`PacketSerializer`序列化/反序列化数据包
- 支持自定义`RealmNexusUpdate`数据包
- 处理`ISideSpecific`接口

#### ClientHandler

处理器基类，提供以下事件：

- `OnCommonPacket`: 通用数据包处理
- `OnS2CPacket`: 服务端到客户端数据包处理
- `OnC2SPacket`: 客户端到服务端数据包处理
- `OnCleaning`: 清理事件（切换服务器时）

### 自定义数据包

`RealmNexusUpdate`是一个自定义数据包，用于：

- 传递客户端真实 IP 地址
- 请求服务器切换
- 自定义服务器连接

数据包结构：

```shell
MessageID: Unused67 (67)
SubType: SubMessageID
  - ClientAddress = 1
  - ChangeSever = 2
  - ChangeCustomizedServer = 3
  - OnlineInfoRequest = 4
  - OnlineInfoResponse = 5
Content: string
Port: ushort (可选，取决于 SubType)
```

## 处理器说明

| 处理器                 | 功能                 |
| ------------------- | ------------------ |
| CommandHandler      | 处理`/server`命令        |
| ConnectionHandler   | 处理连接初始化、玩家同步       |
| CustomPacketHandler | 处理`RealmNexusUpdate`数据包 |
| NpcHandler          | 追踪NPC状态，切换时清理      |
| ProjectileHandler   | 追踪投射物状态，切换时清理      |
| PlayerHandler       | 追踪其他玩家状态，切换时清理     |
| ItemHandler         | 追踪物品状态，切换时清理       |
| PylonHandler        | 追踪传送塔状态，切换时清理      |
| SSCHandler          | 处理服务器端角色数据         |
