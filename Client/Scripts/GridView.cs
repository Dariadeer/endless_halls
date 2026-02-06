using Client.Scripts.Config;
using Godot;
using Shared.Data;

namespace Client.Scripts;

public partial class GridView : Node
{
    [Export]
    public float TileRadius = 40;
    [Export]
    public PackedScene TileScene;
    [Signal]
    public delegate void TileClickedEventHandler(int x, int y);
    private Shared.Data.TileMap _grid;
    public void Initialize(GameContext gameContext)
    {
        _grid = gameContext.World.Grid;
        Render();
    }
    private void Render()
    {
        // foreach(var child in GetChildren())
        // {
        //     child.QueueFree();
        // }

        if(GetChildCount() > 0) return;

        foreach (var tile in _grid.Values)
        {
            var instance = TileScene.Instantiate<TileView>();
            AddChild(instance);
            instance.Position = Coords.ToHexCenter(tile.Pos);
            instance.Name = $"{tile.Pos}";
            instance.Initialize(tile);

            instance.Clicked += OnTileClicked;
        }
        GD.Print("HELLO!");
    }

    public void OnTileClicked(int x, int y)
    {
        EmitSignal(SignalName.TileClicked, [x, y]);
    }
}