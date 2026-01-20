using Godot;

namespace Client.Scripts;

public partial class Camera : Camera2D
{
    [Export]
    public float MoveSpeed { get; set; } = 1;
    [Export]
    public float ZoomSpeed { get; set; } = 1;
    [Export]
    public float ZoomTransitionSpeed { get; set; } = 1;
    [Export]
    public float MagnifyZoomSpeed { get; set; } = 1;

    private float _targetZoom = 1;

    public override void _Process(double delta)
    {
        var fDelta = (float) delta;

        var moveInput = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        var translation = moveInput * fDelta * MoveSpeed;
        Position += translation;

        float zoomTransition = fDelta * ZoomTransitionSpeed;
        float zoom = Zoom.X * (1 - zoomTransition) + _targetZoom * zoomTransition;
        Zoom = new Vector2(zoom, zoom);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMagnifyGesture magnifyGesture)
        {
            var zoomDelta = (magnifyGesture.Factor - 1) * ZoomSpeed * MagnifyZoomSpeed;

            _targetZoom = Mathf.Min(Mathf.Max(Zoom.X * (1 + zoomDelta), 1), 5);
        } 

        if (@event is InputEventMouseButton mouseEvent)
        {
            float zoomDelta = 0;
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.WheelDown:
                    zoomDelta -= 1;
                    break;
                case MouseButton.WheelUp:
                    zoomDelta += 1;
                    break;
            }
            var zoom = Mathf.Min(Mathf.Max(Zoom.X * (1 + zoomDelta * ZoomSpeed), 1), 5);
            _targetZoom = zoom;
        } 
    }

    
}