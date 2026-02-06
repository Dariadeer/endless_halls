using Shared.Network.Messages;

namespace Shared.Network;

public class ServerPing : ISerializable<ServerPing>, IServerMessageable
{
    public required ClientPing ClientPing;
    public int GlobalTick;
    public long GlobalTimeMs;

    public static ServerMessageType MessageType => ServerMessageType.Ping;

    public static ServerPing Decode(BinaryReader reader)
    {
        return new ServerPing
        {
            ClientPing = ClientPing.Decode(reader),
            GlobalTick = reader.ReadInt32(),
            GlobalTimeMs = reader.ReadInt64()
        };
    }

    public void Encode(BinaryWriter writer)
    {
        ClientPing.Encode(writer);
        writer.Write(GlobalTick);
        writer.Write(GlobalTimeMs);
    }
}