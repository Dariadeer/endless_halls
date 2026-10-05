using Godot;
using Shared.Data;

namespace Client.Source;

public partial class TileView : Node2D
{
    private Tile _tile;
    private bool _mouseInBounds = false;
    private bool _isVisibleToTeam;
    private bool _isExploredByTeam;

    [Signal]
    public delegate void ClickedEventHandler(int x, int y);

    public void Initialize(Tile tile)
    {
        _tile = tile;

        var collider = GetNode<Area2D>("TileCollider");
        collider.MouseEntered += OnMouseEntered;
        collider.MouseExited += OnMouseExited;

    }

    public void SetVisionState(bool isVisible, bool isExplored)
    {
        _isVisibleToTeam = isVisible;
        _isExploredByTeam = isExplored;
        Visible = isVisible || isExplored;
        ApplyVisionModulation();
    }

    public void OnMouseEntered()
    {
        _mouseInBounds = true;
        Modulate = new Color("#77aa77");
    }

    public void OnMouseExited()
    {
        _mouseInBounds = false;
        ApplyVisionModulation();
    }

    private void ApplyVisionModulation()
    {
        Modulate = _isVisibleToTeam
            ? Colors.White
            : _isExploredByTeam
                ? new Color(0.45f, 0.45f, 0.45f)
                : Colors.White;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && _mouseInBounds && mouseEvent.ButtonIndex == MouseButton.Left && !mouseEvent.Pressed)
        {
            EmitSignal(SignalName.Clicked, [_tile.Pos.X, _tile.Pos.Y]);
        }
    }
}
