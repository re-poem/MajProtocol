# MajProtocol

Majdata protocol DTOs and serialization helpers.

v1 was extracted from
`MajdataPlay/Assets/Scripts/Scenes/View/Types/` 

## Format

```jsonc
// client -> server
{
  "protocolVersion": 3,
  "requestType": "Play",
  "requestData": { "playMode": "Normal", "startAt": 1.5, "speed": 1.0, "offset": -0.04 }
}

// server -> client
{
  "protocolVersion": 3,
  "responseType": "Event",
  "responseData": { "eventType": "PlayStarted", "summary": { "state": "Playing", "errMsg": "", "timeline": 0 } }
}
```

## Message (v2)

所有 payload 都以原始 JSON 的形式放在 envelope 的 `requestData` / `responseData`
字段中。下表对应 `Types/Request/RequestType.cs`
和 `Types/Response/ResponseType.cs`。

**实际上，用户可在双端约定自己的一套payload，避免局限性。**

### 请求（客户端 → 服务端）

| 类型       | 编码 | 负载             | 说明                                                                                |
|----------|-----:|-----------------|----------------------------------------------------------------------------------|
| `Reset`  | 0    | —               | 将服务端重置为初始空闲状态。                                    |
| `Load`   | 1    | `LoadRequest`   | 加载曲目及可选的封面 / 视频资源（`trackPath`、`imagePath`、`videoPath`）。        |
| `Parse`  | 2    | `ParseRequest`  | 解析 simai 铺面，并应用曲目元数据。                              |
| `Play`   | 3    | `PlayRequest`   | 开始播放。提供播放参数。                                  |
| `Pause`  | 4    | —               | 暂停正在进行的播放。                                              |
| `Resume` | 5    | —               | 继续播放先前暂停的内容。                                                            |
| `Stop`   | 6    | —               | 停止播放并将服务端恢复到非播放状态。                                                  |
| `State`  | 7    | —               | 查询当前服务端状态。                                              |
| `Setting`| 8    | —               | 修改服务端设置。                                                            |

| 负载类型 | 包含字段 | 说明 |
|-----|-----|------|
| `LoadRequest` | TrackPath, ImagePath, VideoPath | 从指定路径加载大的资源（`null` 字段会被忽略）。 |
| `ParseRequest` | SimaiFumen, ClockCount, Offset, <br /> Title, Artist, Level, Designer, Difficulty | Player 解析 simai 文本、时钟数、全局偏移、曲目元数据。 |
| `PlayRequest` | PlayMode, StartAt, Speed, MaidataPath | 播放已解析的曲目。一般地，需先通过 `Parse` 处理后再发 `Play`。 |

### 响应（服务端 → 客户端）

| 类型       | 编码 | 负载             | 说明                                                                  |
|----------|-----:|-----------------|----------------------------------------------------------------------|
| `Error`  | 0    | —               | 上一个请求无法处理。具体错误在ViewSummary.ErrMsg。                                       |
| `Ok`     | 1    | —               | 通用确认应答，用于所有成功处理的请求（无状态变化、或请求方不关心子状态时）。             |
| `Event`  | 2    | `EventResponse` | 服务端主动推送的语义事件：LoadOk / PlayStarted / PlayPaused / PlayResumed / PlayStopped / Heartbeat。取代旧版把子状态硬塞进 ResponseType 的冗余枚举。 |

| 负载类型 | 包含字段 | 说明 |
|-----|-----|------|
| `EventResponse` | EventType, Summary? | EventType 表示事件类型；Summary 是当前 ViewSummary（可选）。 |