using Shared.Network;
using Shared.Network.Messages;

namespace Shared.Data;

public class World : ISnapshot<World>, ISerializable<World>, IServerMessageable
{
    public readonly TileMap Grid;
    public readonly EntityMap Entities;
    public readonly TileMapPathfinder Pathfinder;
    public Action<Entity>? EntityAppeared;
    public Action<int>? EntityDisappeared;

    public static ServerMessageType MessageType => ServerMessageType.WorldState;

    public World(TileMap tileMap, EntityMap entityMap)
    {
        Grid = tileMap;
        Entities = entityMap;
        Pathfinder = new(tileMap);
    }

    public void SummonEntity(Entity entity)
    {
        Entities.AddEntity(entity);
        EntityAppeared?.Invoke(entity);
    }

    public void RemoveEntity(Entity entity)
    {
        Entities.Remove(entity.Id);
        entity.Disappear();
    }

    public void Advance(int tick)
    {
        foreach(var entity in Entities.Values)
        {
            var movement = entity.Movement;
            if(movement.State == MovementState.Moving && movement.End == tick)
            {
                entity.CompleteMovement(tick);
            }
        }
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