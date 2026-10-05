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
    public PackedScene TileScene;
    [Signal]
    public delegate void TileClickedEventHandler(int x, int y);

    private Shared.Data.TileMap _grid;
    private GameContext _context;
    private readonly Dictionary<int2, TileView> _tileViews = [];
    private bool _visionInitialized;
    private bool _hadVisionOrigin;
    private int2 _lastVisionOrigin;

    public void Initialize(GameContext gameContext)
    {
        _context = gameContext;
        _grid = gameContext.World.Grid;
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
            var instance = TileScene.Instantiate<TileView>();
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

        Entity firstEntity = null;
        foreach (var entity in _context.World.Entities.Values)
        {
            firstEntity = entity;
            break;
        }

        if (firstEntity is null)
        {
            if (_visionInitialized && !_hadVisionOrigin)
            {
                return;
            }

            foreach (var tileView in _tileViews.Values)
            {
                tileView.SetVisionVisible(false);
            }

            _hadVisionOrigin = false;
            _visionInitialized = true;
            return;
        }

        var origin = firstEntity.Pos;

        if (_visionInitialized && _hadVisionOrigin && origin == _lastVisionOrigin)
        {
            return;
        }

        var visiblePositions = new HashSet<int2>();
        foreach (var tile in firstEntity.VisionArea)
        {
            visiblePositions.Add(tile.Pos);
        }

        foreach (var (pos, tileView) in _tileViews)
        {
            tileView.SetVisionVisible(visiblePositions.Contains(pos));
        }

        _lastVisionOrigin = origin;
        _hadVisionOrigin = true;
        _visionInitialized = true;
    }

    public void OnTileClicked(int x, int y)
    {
        EmitSignal(SignalName.TileClicked, [x, y]);
    }
}
