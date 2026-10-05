# MajdataWs

Majdata WebSocket wire-protocol DTOs and serialization helpers.

Extracted from
`MajdataPlay/Assets/Scripts/Scenes/View/Types/` 

## Format

```jsonc
// client -> server
{
  "protocolVersion": 1,
  "requestType": "Play",
  "requestData": { "startAt": 1.5, "simaiFumen": "E2", "offset": -0.04, "speed": 1.0 }
}

// server -> client
{
  "protocolVersion": 1,
  "responseType": 203,
  "responseData": { "state": "Playing", "errMsg": "", "timeline": 12.34 }
}
```

## Message

所有 payload 都以原始 JSON 的形式放在 envelope 的 `requestData` / `responseData`
字段中。下表对应 `Types/Request/RequestType.cs`
和 `Types/Response/ResponseType.cs`。

### 请求（客户端 → 服务端）

| 类型       | 编码 | 负载             | 说明                                                                                |
|----------|-----:|-----------------|----------------------------------------------------------------------------------|
| `Reset`  | 0    | —               | 将服务端重置为初始空闲状态。                                    |
| `Load`   | 1    | `LoadRequest`   | 加载曲目及可选的封面 / 视频资源（`trackPath`、`imagePath`、`videoPath`）。        |
| `Parse`  | 2    | —               | ~~解析 simai 数据。~~（空包，目前仅在 `Play` 请求中处理）                              |
| `Play`   | 3    | `PlayRequest`   | 开始播放。提供铺面数据和播放参数                                           |
| `Pause`  | 4    | —               | 暂停正在进行的播放。                                              |
| `Resume` | 5    | —               | 继续播放先前暂停的内容。                                                            |
| `Stop`   | 6    | —               | 停止播放并将服务端恢复到非播放状态。                                                  |
| `State`  | 7    | —               | 查询当前服务端状态；服务端以 `Heartbeat` 响应作为回复。                              |

| 负载类型 |     包含字段    | 说明                                 |
|-----|-----------------|----------------------------------------------------------------------------------|
| `LoadRequest` | TrackPath, ImagePath, VideoPath | 从指定路径加载大的资源                                                                         |
| `PlayRequest` | StartAt, SimaiFumen, Offset, Speed | 加载Fumen并从指定位置播放                                                                        |
### 响应（服务端 → 客户端）

| 类型             | 编码 | 负载            | 说明                                                                                          |
|----------------|-----:|--------------|----------------------------------------------------------------------------------------------|
| `Ok`           | 200  | —            | 通用确认应答，用于没有专属响应的请求（如 `Reset`、`Parse`、`State`）。                        |
| `PlayStarted`  | 201  | —            | 确认 `Play` 请求已接受，播放已开始。                                                          |
| `PlayResumed`  | 202  | —            | 确认 `Resume` 请求已接受。                                                                    |
| `Heartbeat`    | 203  | `ViewSummary`| 周期性状态快照：当前 `state`（`ViewStatus`）、可选的 `errMsg`，以及播放 `timeline`。           |
| `PlayPaused`   | 204  | —            | 确认 `Pause` 请求已接受。                                                                    |
| `PlayStopped`  | 205  | —            | 确认 `Stop` 请求已接受。                                                                      |
| `LoadOk`       | 206  | —            | 确认 `Load` 请求已成功完成。                                                                  |
| `Error`        | 400  | —            | 上一个请求无法处理。请查看最近一次 `Heartbeat.errMsg` 获取详细信息。                          |