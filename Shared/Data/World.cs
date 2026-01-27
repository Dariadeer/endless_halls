using Shared.Network;
using Shared.Network.Messages;

namespace Shared.Data;

public class World : ISnapshot<World>, ISerializable<World>, IServerMessageable
{
    public readonly TileMap Grid;
    public readonly EntityMap Entities;

    public Action<Entity> EntitySummoned;

    public static ServerMessageType MessageType => ServerMessageType.WorldState;

    public World(TileMap tileMap, EntityMap entityMap)
    {
        Grid = tileMap;
        Entities = entityMap;
    }

    public void SummonEntity(Entity entity)
    {
        Entities.AddEntity(entity);
        EntitySummoned.Invoke(entity);
    }

    public World Copy()
    {
        return new World(
            Grid.Copy(),
            Entities.Copy()
        );
    }

    public void Encode(BinaryWriter writer)
    {
        Grid.Encode(writer);
        Entities.Encode(writer);
    }

    public static World Decode(BinaryReader reader)
    {
        return new World(
            TileMap.Decode(reader),
            EntityMap.Decode(reader)
        );
    }
}