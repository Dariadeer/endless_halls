namespace Shared.Network.Messages;

// Server Messages: MessageType > 128
// Client Messages: MessageType < 128
public enum ClientMessageType : byte
{
    Ping = 0,
    Command = 1,
    WorldData = 2,
    Login = 3
}