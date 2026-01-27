using Shared.Math;
using Shared.Network;

namespace Shared.Data;

public class Entity : ISnapshot<Entity>, ISerializable<Entity>
{
    public readonly int Id;
    public int TeamId;
    public Int2 Pos;
    public Movement Movement;

    public Entity(int id = 0)
    {
        Id = id;
    }

    public Entity Copy()
    {
        return new Entity(Id)
        {
            TeamId = TeamId,
            Pos = Pos,
            Movement = Movement
        };
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(Id);
        writer.Write(TeamId);
        Pos.Encode(writer);
        Movement.Encode(writer);
    }

    public static Entity Decode(BinaryReader reader)
    {
        return new Entity(reader.ReadInt32())
        {
            TeamId = reader.ReadInt32(),
            Pos = Int2.Decode(reader),
            Movement = Movement.Decode(reader)
        };
    }
}