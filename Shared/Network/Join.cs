using Shared.Network.Messages;

namespace Shared.Network;

public class Join : ISerializable<Join>, IClientMessageable
{
    public static ClientMessageType MessageType => ClientMessageType.WorldData;

    public Join()
    {
        
    }
    public void Encode(BinaryWriter writer)
    {
        
    }

    public static Join Decode(BinaryReader reader)
    {
        return new Join();
    }
}