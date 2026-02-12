using Shared.Data;
using Shared.MyMath;

namespace Tests.Utils;

public class WorldGenerator
{
    public static World Empty()
    {
        TileMap grid = new();
        grid.Generate(5);

        World world = new(grid, []);

        return world;
    }

    public static World Simple()
    {
        TileMap grid = new();
        grid.Generate(5);

        World world = new(grid, []);

        var entities = EntityGenerator.Dict(30);
        
        foreach (var entry in entities)
        {
            world.Entities.AddEntity(entry.Value);
        }

        return world;
    }
}

public class EntityGenerator
{
    public static Entity One(int id)
    {
        return new Entity(id) {
            TeamId = 0,
            Pos = new Int2(0, 0),
            Movement = new Movement(123, 234, new Int2(13, 132))
        };
    }

    public static Dictionary<int, Entity> Dict(int n)
    {
        Dictionary<int, Entity> entities = [];

        for(int i = 0; i < n; i++)
        {
            entities[i] = One(i);
        }

        return entities;
    }
}