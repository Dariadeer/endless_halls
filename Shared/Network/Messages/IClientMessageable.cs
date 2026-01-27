namespace Shared.Network.Messages;

public interface IClientMessageable
{
    public static abstract ClientMessageType MessageType { get; }
}