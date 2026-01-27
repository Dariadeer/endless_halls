using Shared.Network.Messages;

namespace Shared.Network.Messages;

public interface IServerMessageable
{
    public static abstract ServerMessageType MessageType { get; }
}