using Godot;
using Shared.Data;

namespace Client.Scripts;

public partial class GridView : Node
{
    [Export]
    public float TileRadius = 40;
    private HexGrid _grid;
    public void Initialise(GameContext gameContext)
    {
        _grid = gameContext.World.Grid;

        GD.Print("Rendering...");
        Render();
    }
    private void Render()
    {
        var scene = GD.Load<PackedScene>("res://Scenes/Tile.tscn");
        foreach (var tile in _grid.GetAll())
        {
            GD.Print(tile.Pos);
            var instance = scene.Instantiate<Node2D>();
            AddChild(instance);
            instance.Position = Utils.ToHexCenter(tile.Pos, TileRadius);
        }
    }
}