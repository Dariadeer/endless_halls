using Shared.Network;
using Shared.Network.Messages;

namespace Shared.Data.Commands;

public class DisappearCommand : ICommand, ISerializable<DisappearCommand>, IServerMessageable
{
    public int Id { get; }
    public int Tick { get; }
    public readonly int EntityId;
    public static ServerMessageType MessageType => ServerMessageType.Disappearance;

    public DisappearCommand(int id, int tick, int entityId) 
    {
        Id = id;
        Tick = tick;
        EntityId = entityId;
    }

    public static DisappearCommand Decode(BinaryReader reader)
    {
        return new(
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32()
        );
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(Id);
        writer.Write(Tick);
        writer.Write(EntityId);
    }
}