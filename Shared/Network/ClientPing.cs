using Shared.Network.Messages;

namespace Shared.Network;

public class ClientPing : ISerializable<ClientPing>, IClientMessageable
{
    public int Tick;
    public byte Id;

    public static ClientMessageType MessageType => ClientMessageType.Ping;

    public static ClientPing Decode(BinaryReader reader)
    {
        return new ClientPing
        {
           Tick = reader.ReadInt32(),
           Id = reader.ReadByte() 
        };
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(Tick);
        writer.Write(Id);
    }
}