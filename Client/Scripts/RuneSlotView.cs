using Godot;
using Shared.MyMath;

namespace Client.Scripts;

public partial class RuneSlotView : Node2D
{
    [Signal]
    public delegate void ClickedEventHandler(int x, int y, int mode);
    private bool _mouseInBounds = false;
    private Int2 _pos;

    public void Initialize(Int2 pos)
    {
        _pos = pos;
    }
    public override void _Ready()
    {
        var collider = GetNode<Area2D>("Area2D");
        collider.MouseEntered += OnMouseEntered;
        collider.MouseExited += OnMouseExited;
    }

    public void OnMouseEntered()
    {
        _mouseInBounds = true;
    }

    public void OnMouseExited()
    {
        _mouseInBounds = false;
    }

    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseEvent && _mouseInBounds && !mouseEvent.Pressed)
        {
            EmitSignal(SignalName.Clicked, [_pos.X, _pos.Y, mouseEvent.ShiftPressed && mouseEvent.ButtonIndex == MouseButton.Left ? 1 : mouseEvent.ButtonIndex == MouseButton.Left ? 0 : 2]);
        }
    }
}