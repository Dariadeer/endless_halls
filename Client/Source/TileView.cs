using Godot;
using Shared.Data;

namespace Client.Scripts;

public partial class TileView : Node2D
{
    private Tile _tile;
    private bool _mouseInBounds = false;

    [Signal]
    public delegate void ClickedEventHandler(int x, int y);

    public void Initialize(Tile tile)
    {
        _tile = tile;

        var collider = GetNode<Area2D>("TileCollider");
        collider.MouseEntered += OnMouseEntered;
        collider.MouseExited += OnMouseExited;

        if (!_tile.IsWalkable())
        {
            GetNode<Polygon2D>("Polygon2D").Color = new Color(0.7f ,0.7f, 0.7f);
        }
    }

    public void OnMouseEntered()
    {
        _mouseInBounds = true;
        Modulate = new Color("#77aa77");
    }

    public void OnMouseExited()
    {
        _mouseInBounds = false;
        Modulate = new Color(1, 1, 1);
    }

    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseEvent && _mouseInBounds && mouseEvent.ButtonIndex == MouseButton.Left && !mouseEvent.Pressed)
        {
            EmitSignal(SignalName.Clicked, [_tile.Pos.X, _tile.Pos.Y]);
        }
    }
}