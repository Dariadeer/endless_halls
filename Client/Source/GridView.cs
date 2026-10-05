using Client.Source.Data;
using Godot;
using System.Collections.Generic;
using Shared.Data;
using Shared.MyMath;

namespace Client.Source;

public partial class GridView : Node
{
    [Export]
    public float TileRadius = 40;
    [Export]
    public PackedScene FloorTileScene;
    [Export]
    public PackedScene WallTileScene;
    [Signal]
    public delegate void TileClickedEventHandler(int x, int y);

    private Shared.Data.TileMap _grid;
    private GameContext _context;
    private readonly Dictionary<int2, TileView> _tileViews = [];
    private bool _visionInitialized;
    private int _lastVisionRevision = -1;
    private int _lastVisionTeam;

    public void Initialize(GameContext gameContext)
    {
        _context = gameContext;
        _grid = gameContext.World.Grid;
        _visionInitialized = false;
        CallDeferred("Render");
    }

    private void Render()
    {
        // foreach(var child in GetChildren())
        // {
        //     child.QueueFree();
        // }

        if (GetChildCount() > 0)
        {
            UpdateVision();
            return;
        }

        foreach (var tile in _grid.Values)
        {
            var tileScene = tile.IsWalkable() ? FloorTileScene : WallTileScene;
            var instance = tileScene.Instantiate<TileView>();
            AddChild(instance);
            instance.Position = Coords.ToHexCenter(tile.Pos);
            instance.Name = $"{tile.Pos}";
            instance.Initialize(tile);
            _tileViews[tile.Pos] = instance;

            instance.Clicked += OnTileClicked;
        }

        _visionInitialized = false;
        UpdateVision();
    }

    public override void _Process(double delta)
    {
        UpdateVision();
    }

    private void UpdateVision()
    {
        if (_context is null || _grid is null)
        {
            return;
        }

        var world = _context.World;
        int teamId = _context.PlayerTeam;
        if (_visionInitialized
            && _lastVisionRevision == world.VisionRevision
            && _lastVisionTeam == teamId)
        {
            return;
        }

        foreach (var (pos, tileView) in _tileViews)
        {
            tileView.SetVisionState(
                world.IsTileInCurrentVisionForTeam(teamId, pos),
                world.IsTileExploredByTeam(teamId, pos));
        }

        _lastVisionRevision = world.VisionRevision;
        _lastVisionTeam = teamId;
        _visionInitialized = true;
    }

    public void OnTileClicked(int x, int y)
    {
        EmitSignal(SignalName.TileClicked, [x, y]);
    }
}
