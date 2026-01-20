using Shared.Math;

namespace Shared.Data;

public class Entity
{
    public readonly int Id;
    public EntityData Data;

    public Entity(int id, EntityData data)
    {
        Id = id;
        Data = data;
    }

    public void Move(Int2 pos)
    {
        Data.Pos = pos;
    }
}