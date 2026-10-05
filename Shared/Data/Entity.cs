using Shared.MyMath;
using Shared.Network;

namespace Shared.Data;

public class Entity(int id = 0) : ISnapshot<Entity>, ISerializable<Entity>
{
    public readonly int Id = id;
    public int TeamId;
    private int2 _pos;
    private TileMap? _grid;
    public int2 Pos
    {
        get => _pos;
        set
        {
            if (_pos == value)
            {
                return;
            }

            _pos = value;
            RecalculateVisionArea();
        }
    }

    public IReadOnlyList<Tile> VisionArea { get; private set; } = Array.Empty<Tile>();
    public Movement Movement = Movement.Idle;
    public MoveQueue Path = new();
    public Action? PathUpdated;
    public Action? Disappeared;

    public void AttachGrid(TileMap grid)
    {
        _grid = grid;
        RecalculateVisionArea();
    }

    private void RecalculateVisionArea()
    {
        if (_grid is not null)
        {
            VisionArea = _grid.GetVisionArea(_pos);
        }
    }

    public void AppendMove(int2 to)
    {
        Path.Enqueue(to);
    }

    public void AppendPath(IEnumerable<int2> tos, int tick)
    {
        foreach (var to in tos)
        {
            if (Movement.State == MovementState.Idle)
            {
                Movement = new Movement(
                    tick, tick + 30, to
                );
            }
            else
            {
                Path.Enqueue(to);
            }
        }
        PathUpdated?.Invoke();
    }

    public void ClearPath()
    {
        Path.Clear();
        PathUpdated?.Invoke();
    }

    public void CompleteMovement(int tick)
    {
        Pos = Movement.To;
        if (Path.Count > 0)
        {
            Movement = new Movement(tick, tick + 30, Path.Dequeue());
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
            Pos = int2.Decode(reader),
            Movement = Movement.Decode(reader),
            Path = MoveQueue.Decode(reader)
        };
    }

    public void Disappear()
    {
        Disappeared?.Invoke();
    }
}
