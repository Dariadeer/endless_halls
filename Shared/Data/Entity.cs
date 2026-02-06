using Shared.Date;
using Shared.Math;
using Shared.Network;

namespace Shared.Data;

public class Entity : ISnapshot<Entity>, ISerializable<Entity>
{
    public readonly int Id;
    public int TeamId;
    public Int2 Pos;
    public Movement Movement = Movement.Idle;
    public MoveQueue Path = new();
    public Action? PathUpdated;

    public Entity(int id = 0)
    {
        Id = id;
    }

    public void AppendMove(Int2 to)
    {
        Path.Enqueue(to);
    }

    public void AppendPath(IEnumerable<Int2> tos, int tick)
    {
        foreach(var to in tos)
        {
            if (Movement.State == MovementState.Idle)
            {
                Movement = new Movement(
                    tick, tick + 50, to
                );
            }
            else
            {
                Path.Enqueue(to);
            }
        }
        PathUpdated?.Invoke();
    }

    public void CompleteMovement(int tick)
    {
        Pos = Movement.To;
        if(Path.Count > 0)
        {
            Movement = new Movement(tick, tick + 50, Path.Dequeue());
        }
        else
        {
            Movement = Movement.Idle;
        }
        PathUpdated?.Invoke();
    }

    public Entity Copy()
    {
        return new Entity(Id)
        {
            TeamId = TeamId,
            Pos = Pos,
            Movement = Movement,
            Path = Path.Copy()
        };
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(Id);
        writer.Write(TeamId);
        Pos.Encode(writer);
        Movement.Encode(writer);
        Path.Encode(writer);
    }

    public static Entity Decode(BinaryReader reader)
    {
        return new Entity(reader.ReadInt32())
        {
            TeamId = reader.ReadInt32(),
            Pos = Int2.Decode(reader),
            Movement = Movement.Decode(reader),
            Path = MoveQueue.Decode(reader)
        };
    }
}