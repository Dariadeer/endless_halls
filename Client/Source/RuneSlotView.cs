using System.Collections.Generic;
using Godot;
using Shared.MyMath;

namespace Client.Scripts;

public partial class RuneSlotView : Node2D
{
	[Signal]
	public delegate void ClickedEventHandler(int x, int y, int mode);
	private bool _mouseInBounds = false;
	private int2 _pos;

	public void Initialize(int2 pos)
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
		if (@event is InputEventMouseButton mouseEvent
			&& _mouseInBounds
			&& mouseEvent.Pressed)
		{
			List<Variant> args = [_pos.X, _pos.Y];
			if (mouseEvent.ButtonIndex == MouseButton.Left)
			{
				args.Add(0);
			}
			else if (mouseEvent.ButtonIndex == MouseButton.Right)
			{
				args.Add(1);
			}
			else
			{
				return;
			}

			EmitSignal(SignalName.Clicked, args.ToArray());
		}
	}
}
