using System;
using System.Collections.Generic;
using Client.Scripts.Data;
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
        _context.World.EntityAppeared += OnEntitySummoned;

        CallDeferred("Render");
    }

    public void Render()
    {
        foreach(var child in GetChildren())
        {
            child.QueueFree();
        }
        
        foreach (var entity in _context.World.Entities.Values)
        {
            CreateEntity(entity);
        }
    }

    public EntityView CreateEntity(Entity entity)
    {
        var instance = EntityScene.Instantiate<EntityView>();
        instance.Initialize(_context, entity);
        AddChild(instance);

        return instance;
    }

    public void OnEntitySummoned(Entity entity)
    {
        var entityView = CreateEntity(entity);
        GD.Print($"{entity.Id}, {_context.Camera.EntityIdFollowed}");
        if(entity.Id == _context.Camera.EntityIdFollowed || entity.TeamId == _context.Camera.EntityTeamIdFollowed)
        {
            _context.Camera.EntityToFollow = entityView;
        }
    }
}