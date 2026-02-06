using Shared.Network.Messages;

namespace Shared.Network;

public class JoinRequest : ISerializable<JoinRequest>, IClientMessageable
{
    public string Name;

    public static ClientMessageType MessageType => ClientMessageType.Join;

    public JoinRequest(string name)
    {
        Name = name;
    }
    public void Encode(BinaryWriter writer)
    {
        writer.Write(Name);
    }

    public static JoinRequest Decode(BinaryReader reader)
    {
        return new JoinRequest(
            reader.ReadString()
        );
    }
}