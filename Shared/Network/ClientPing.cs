using Shared.Network.Messages;

namespace Shared.Network;

public class ClientPing : ISerializable<ClientPing>, IClientMessageable
{
    public int LocalTick;
    public long LocalTime;

    public static ClientMessageType MessageType => ClientMessageType.Ping;

    public static ClientPing Decode(BinaryReader reader)
    {
        return new ClientPing
        {
           LocalTick = reader.ReadInt32(),
           LocalTime = reader.ReadInt64() 
        };
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(LocalTick);
        writer.Write(LocalTime);
    }
}