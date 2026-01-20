namespace Client.Scripts;

using Godot;
using Shared.Data;
using Shared.Logic;
using Shared.Math;

public partial class WorldView : Node
{
	public Loop Loop;
	[Export]
	public GridView GridView;
	public override void _Ready()
	{
		HexGrid grid = new();
		grid.Generate(5);

		var gameContext = new GameContext{
			World = new World(grid)
		};
		

		GridView.Initialise(gameContext);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
