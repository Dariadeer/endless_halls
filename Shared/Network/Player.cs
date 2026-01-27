using Shared.Network.Messages;

namespace Shared.Network;

public class Player : ISerializable<Player>, IServerMessageable
{
    public int Id;
    public string Name;

    public static ServerMessageType MessageType => ServerMessageType.PlayerData;

    public Player(int id, string name)
    {
        Id = id;
        Name = name;
    }
    public void Encode(BinaryWriter writer)
    {
        writer.Write(Id);
        writer.Write(Name);
    }

    public static Player Decode(BinaryReader reader)
    {
        return new Player(
            reader.ReadInt32(),
            reader.ReadString()
        );
    }
}