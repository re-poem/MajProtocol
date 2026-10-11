namespace MajProtocol.Standard;

public enum OpCode : byte
{
    /// <summary>
    /// <para>
    /// Client establishes connection to the server.
    /// </para>
    /// </summary>
    Handshake = 0,

    /// <summary>
    /// <para>
    /// Client sends file instructions to the server.
    /// </para>
    /// </summary>
    Files = 1,

    /// <summary>
    /// <para>
    /// Client sends simai contents to the server.
    /// </para>
    /// </summary>
    Simai = 2,

    /// <summary>
    /// <para>
    /// Server starts chart playback.
    /// </para>
    /// </summary>
    Play = 3,

    /// <summary>
    /// <para>
    /// Server stops chart playback.
    /// </para>
    /// </summary>
    Pause = 4
}
