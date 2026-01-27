namespace Shared.Network.Messages;

public interface IRequestable
{
    public static abstract RequestType RequestType { get; }
}