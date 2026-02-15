using Shared.Network;

namespace Shared.Data;

public class EntityMap : Dictionary<int, Entity>, ISnapshot<EntityMap>, ISerializable<EntityMap>
{
    public void AddEntity(Entity entity)
    {
        Add(entity.Id, entity);
    }
    public EntityMap Copy()
    {
        var clone = new EntityMap();
        
        foreach (var entity in Values)
        {
            clone[entity.Id] = entity.Copy();
        }

        return clone;
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(Count);
        
        foreach (var entity in Values)
        {
            entity.Encode(writer);
        }
    }

    public static EntityMap Decode(BinaryReader reader)
    {
        int count = reader.ReadInt32();

        var map = new EntityMap();

        for (int i = 0; i < count; i++)
        {
            map.AddEntity(Entity.Decode(reader));
        }

        return map;
    }
}