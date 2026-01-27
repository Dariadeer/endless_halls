using Client.Scripts.Config;
using Godot;
using Shared.Data;

namespace Client.Scripts;

public partial class EntityView : Node2D
{
    private Entity _entity;
    private GameContext _context;
    public void Initialize(GameContext context, Entity entity)
    {
        _context = context;
        _entity = entity;
        Position = Coords.ToHexCenter(entity.Pos, Globals.TileRadius);
    }

    public override void _Process(double delta)
    {
        if(_entity.Movement.State == MovementState.Moving)
        {
            Movement movement = _entity.Movement;
            var tileStart = Coords.ToHexCenter(_entity.Pos, Globals.TileRadius);
            var tileEnd = Coords.ToHexCenter(movement.To, Globals.TileRadius);
            var timeStart = _context.CalculateTickTime(movement.Start);
            var timeEnd = _context.CalculateTickTime(movement.End);
            var timeNow = Time.GetUnixTimeFromSystem();
            // GD.Print($"{timeNow} in [{timeStart}, {timeEnd}]");
            Position = tileStart.Lerp(tileEnd, (float) ((timeNow - timeStart) / (timeEnd - timeStart)));
        } 
        else
        {
            Position = Coords.ToHexCenter(_entity.Pos, Globals.TileRadius);
        }
    }
}