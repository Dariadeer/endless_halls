namespace Client.Scripts;

using System;
using Client.Scripts.Utils;
using Godot;
using Shared.Data;
using Shared.Logic;
using Shared.Data.Commands;
using Shared.MyMath;
using Client.Scripts.Data;

public partial class LocalWorldView : Node
{
    private Loop _loop;
    [Export]
    public GridView GridView;
    [Export]
    public EntityManager EntityManager;
    [Export]
    public Camera Camera;
    private GameContext _context;

    public void Initialize()
    {
        // SetPhysicsProcess(false);

        Shared.Data.TileMap grid = new();
        grid.Generate(20);

        World world = new(grid, []);

        _loop = new Loop(world)
        {
            Logger = new GDLogger()
        };

        _loop.WorldStateRecovered += Reinitialize;

        Reinitialize(world);

        _loop.InsertCommand(new AppearCommand(
            0, 0,
            new Entity(0)
            {
                TeamId = 0,
                Pos = new Int2(0, 0),
                Movement = new Movement()
            }
        ));

        Camera.EntityIdFollowed = 0;

        GridView.TileClicked += OnTileClicked;
    }

    public void Reinitialize(World world)
    {
        _context = new GameContext
        {
            World = world,
            CurrentTick = _loop.Tick,
            TimeStart = _context != null ? _context.TimeStart : DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            LastTickProcessed = _loop.Tick,
            Camera = Camera
        };


        GridView.Initialize(_context);

        EntityManager.Initialize(_context);
    }


    public override void _Process(double delta)
    {
        var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        while (now > _context.CalculateTickTime(_context.CurrentTick))
        {
            _loop.Update();
            _context.CurrentTick++;
        }
    }

    public void OnTileClicked(int x, int y)
    {
        GD.Print($"Tile was clicked at {new Int2(x, y)}");

        _loop.InsertCommand(new MoveCommand(
            0, _context.CurrentTick, 0, new Int2(x, y)
        ));
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {

            switch (keyEvent.Keycode)
            {
                case Key.Space:
                    _loop.RecoverState(0);
                    _context.TimeStart = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    break;
                // case Key.Escape:
                // 	await _client.DisconnectAsync();
                // 	Main.GoToMenu(this);
                // 	break;
                case Key.E:
                    _loop.InsertCommand(new HaltCommand(0, _loop.Tick + 1, 0));
                    break;
            }
        }
    }
}
