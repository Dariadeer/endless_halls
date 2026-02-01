namespace Shared.Network.Messages;

// Server Messages: MessageType > 128
// Client Messages: MessageType < 128
public enum ClientMessageType : byte
{
    Ping = 0,
    Move = 1,
    WorldData = 2,
    Join = 3
}