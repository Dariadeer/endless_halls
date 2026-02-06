namespace Shared.Network.Messages;

// Server Messages: MessageType > 128
// Client Messages: MessageType < 128
public enum ServerMessageType : byte
{
    Ping = 0,
    Commands = 1,
    WorldState = 2,
    PlayerData = 3,
    Movement = 4,
    Appearance = 5
}