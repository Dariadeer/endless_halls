using System;
using Godot;
using Shared.Math;

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

	[Signal]
    public delegate void TileClickedEventHandler(int x, int y);

    private float _targetZoom = 1;
    private bool _mouseDown;
    private Vector2 _mouseDownLocalPos;
    private Vector2 _mouseMoveLocalPos;
    private Vector2 _mouseDownGlobalPos;
    private bool _mouseMoved;

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
        switch (@event)
        {
            case InputEventMagnifyGesture magnifyGesture:
                HandleMagnifyEvent(magnifyGesture);
                break;
            
            case InputEventMouseButton mouseEvent:
                HandleMouseButtonEvent(mouseEvent);
                break;
            case InputEventMouseMotion motionEvent:
                HandleMouseMotionEvent(motionEvent);
                break;
        }
    }

    private void HandleMagnifyEvent(InputEventMagnifyGesture magnifyGesture)
    {
        float zoomDelta = (magnifyGesture.Factor - 1) * ZoomSpeed * MagnifyZoomSpeed;
        _targetZoom = Mathf.Min(Mathf.Max(Zoom.X * (1 + zoomDelta), 1), 5);
    }

    private void HandleMouseButtonEvent(InputEventMouseButton mouseEvent)
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
            case MouseButton.Left or MouseButton.Right or MouseButton.Middle:
                if(mouseEvent.Pressed)
                {
                    _mouseDown = true;
                    _mouseDownLocalPos = mouseEvent.Position;
                    _mouseMoveLocalPos = mouseEvent.Position;
                    _mouseDownGlobalPos = Position;
                } 
                else
                {
                    if((_mouseDownLocalPos - _mouseMoveLocalPos).Length() < 15)
                    {
                        Int2 tileHexPos = Coords.ToHexCoords(GetGlobalMousePosition(), 40);
                        EmitSignal(SignalName.TileClicked, [tileHexPos.X, tileHexPos.Y]);
                    }
                    _mouseDown = false;
                    _mouseMoved = false;
                }
                break;
                
        }
        var zoom = Mathf.Min(Mathf.Max(Zoom.X * (1 + zoomDelta * ZoomSpeed), 1), 5);
        _targetZoom = zoom;
    }

    private void HandleMouseMotionEvent(InputEventMouseMotion motionEvent)
    {
        if(_mouseDown)
        {
            _mouseMoved = true;
            _mouseMoveLocalPos = motionEvent.Position;
            Position = _mouseDownGlobalPos + (_mouseDownLocalPos - _mouseMoveLocalPos) / Zoom;
        }
    }
    
}