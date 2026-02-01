using Shared.Math;
using Shared.Network;
using Shared.Network.Messages;

namespace Shared.Data.Commands;

public class MoveCommand : ICommand, ISerializable<MoveCommand>, IClientMessageable, IServerMessageable
{
    public int Id { get; }
    public int Tick { get; }

    public static ClientMessageType MessageType => ClientMessageType.Move;

    static ServerMessageType IServerMessageable.MessageType => ServerMessageType.Move;

    public readonly int EntityId;
    public readonly Int2 To;

    public MoveCommand(int id, int tick, int entityId, Int2 to) 
    {
        EntityId = entityId;
        Tick = tick;
        Id = id;
        To = to;
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(Id);
        writer.Write(Tick);
        writer.Write(EntityId);
        To.Encode(writer);
    }

    public static MoveCommand Decode(BinaryReader reader)
    {
        return new MoveCommand(
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            Int2.Decode(reader)
        );
    }
}