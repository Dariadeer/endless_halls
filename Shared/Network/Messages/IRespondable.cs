using Shared.Network.Messages;

namespace Shared.Network;

public interface IRespondable
{
    public static abstract ResponseType ResponseType { get; }
}