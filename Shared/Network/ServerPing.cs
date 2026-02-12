using Shared.Network.Messages;

namespace Shared.Network;

public class ServerPing : ISerializable<ServerPing>, IServerMessageable
{
    public byte Id;

    public static ServerMessageType MessageType => ServerMessageType.Ping;

    public static ServerPing Decode(BinaryReader reader)
    {
        return new ServerPing
        {
            Id = reader.ReadByte()
        };
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(Id);
    }
}