using System;
using System.Collections.Generic;
using Godot;
using Shared.Data;

namespace Client.Scripts;

public partial class EntityManager : Node2D
{
    [Export]
    public PackedScene EntityScene;
    private GameContext _context;
    public void Initialize(GameContext context)
    {
        _context = context;
        _context.World.EntitySummoned += OnEntitySummoned;

        Render(context.World.Entities);
    }

    public void Render(EntityMap entityMap)
    {
        foreach(var child in GetChildren())
        {
            child.QueueFree();
        }
        
        foreach (var entity in entityMap.Values)
        {
            CreateEntity(entity);
        }
    }

    public void CreateEntity(Entity entity)
    {
        var instance = EntityScene.Instantiate<EntityView>();
        instance.Initialize(_context, entity);
        AddChild(instance);
    }

    public void OnEntitySummoned(Entity entity)
    {
        CreateEntity(entity);
    }
}