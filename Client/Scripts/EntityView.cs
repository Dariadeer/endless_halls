using System;
using Client.Scripts.Config;
using Godot;
using Shared.Data;
using Shared.Math;

namespace Client.Scripts;

public partial class EntityView : Node2D
{
    private Entity _entity;
    private GameContext _context;
    private Line2D _pathLine;

    public override void _Ready()
    {
        _pathLine = GetNode<Line2D>("PathLine");
    }

    public void Initialize(GameContext context, Entity entity)
    {
        _context = context;
        _entity = entity;
        _entity.PathUpdated += OnPathUpdated;
        Position = Coords.ToHexCenter(entity.Pos);
    }

    public override void _Process(double delta)
    {
        if(_entity.Movement.State == MovementState.Moving)
        {
            Movement movement = _entity.Movement;
            var tileStart = Coords.ToHexCenter(_entity.Pos);
            var tileEnd = Coords.ToHexCenter(movement.To);
            var timeStart = _context.CalculateTickTime(movement.Start);
            var timeEnd = _context.CalculateTickTime(movement.End);
            var timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Position = tileStart.Lerp(tileEnd, Mathf.Min((timeNow - timeStart) / (float) (timeEnd - timeStart), 1f));
        } 
        else
        {
            Position = Coords.ToHexCenter(_entity.Pos);
        }

        lock(_pathLine)
        {
            if(_pathLine.Points.Length != 0)
            {
                _pathLine.SetPointPosition(0, Position);
            }
            _pathLine.GlobalPosition = Vector2.Zero;
        }
        
    }

    public void OnPathUpdated()
    {
        try
        {
            _pathLine.ClearPoints();

            if(_entity.Movement.State == MovementState.Idle) return;

            _pathLine.AddPoint(Coords.ToHexCenter(_entity.Pos));
            _pathLine.AddPoint(Coords.ToHexCenter(_entity.Movement.To));

            foreach (var next in _entity.Path)
            {
                _pathLine.AddPoint(Coords.ToHexCenter(next));
            }
        }
        catch { }
    }
}