using Shared.Network.Messages;

namespace Shared.Network;

public class Join : ISerializable<Join>, IRequestable
{
    public static RequestType RequestType => RequestType.WorldData;

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