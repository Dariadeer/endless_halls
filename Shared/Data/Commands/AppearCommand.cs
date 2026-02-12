using Shared.Network;
using Shared.Data;
using Shared.MyMath;
using Shared.Network.Messages;

namespace Shared.Data.Commands;

public class AppearCommand : ICommand, ISerializable<AppearCommand>, IServerMessageable
{
    public int Id { get; }
    public int Tick { get; }

    public static ServerMessageType MessageType => ServerMessageType.Appearance;

    public readonly Entity Entity;
    public readonly Int2 To;

    public AppearCommand(int id, int tick, Entity summonee)
    {
        Id = id;
        Tick = tick;
        Entity = summonee;
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(Id);
        writer.Write(Tick);
        Entity.Encode(writer);
        To.Encode(writer);
    }

    public static AppearCommand Decode(BinaryReader reader)
    {
        return new AppearCommand(
            reader.ReadInt32(),
            reader.ReadInt32(),
            Entity.Decode(reader)
        );
    }
}