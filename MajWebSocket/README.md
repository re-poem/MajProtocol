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
  "responseType": "Heartbeat",
  "responseData": { "state": "Playing", "errMsg": "", "timeline": 12.34 }
}
```