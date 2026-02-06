using Shared.Network.Messages;

namespace Shared.Network;

public class WorldDataRequest : ISerializable<WorldDataRequest>, IClientMessageable
{
    public static ClientMessageType MessageType => ClientMessageType.WorldData;

    public WorldDataRequest()
    {
        
    }
    public void Encode(BinaryWriter writer)
    {
        
    }

    public static WorldDataRequest Decode(BinaryReader reader)
    {
        return new WorldDataRequest();
    }
}