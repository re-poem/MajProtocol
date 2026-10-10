namespace MajProtocol.Types.Response
{
    /// <summary>
    /// 服务端主动推送的语义事件。ResponseType.Event 时 <c>responseData</c> 通常是 EventResponse。
    /// 取代旧版把子状态硬塞进 ResponseType 的冗余枚举，
    /// 让 ResponseType 只表达协议层"成功 / 失败 / 主动事件"三种语义，事件类型放 EventType 里。
    /// </summary>
    public readonly struct EventResponse
    {
        /// <summary>事件类型（Load 完成、播放开始、暂停、恢复、停止、心跳）。</summary>
        public EventType EventType { get; init; }

        /// <summary>事件附带的状态信息（含当前 ViewStatus + Timeline）。</summary>
        public ViewSummary? Summary { get; init; }
    }

    public enum EventType
    {
        LoadOk,
        PlayStarted,
        PlayPaused,
        PlayResumed,
        PlayStopped,
        Heartbeat,
    }
}