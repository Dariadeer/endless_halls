namespace Client.Scripts;

using System;
using System.Collections.Generic;
using Client.Scripts.Utils;
using Godot;
using Shared.Data;
using Shared.Logic;
using Shared.Data.Commands;
using Shared.Math;
using Shared.Utils;
using System.Text.RegularExpressions;
using Client.Scripts.Network;
using Shared.Network;
using Shared.Network.Messages;

public partial class RemoteWorldView : Node
{
	private Loop _loop;
	[Export]
	public GridView GridView;
	[Export]
	public EntityManager EntityManager;
	private GameContext _context;

	private GameClient _client;
    private string _host;
    private int _port;
	private Player _player;

	public async void Initialize(string address)
	{
        // SetPhysicsProcess(false);

		
        // Shared.Data.TileMap grid = new();
		// grid.Generate(5);

		// World world = new(grid, []);

		// _loop = new Loop(world)
        // {
        //     Logger = new GDLogger()
        // };

		// _loop.WorldStateRecovered += Reinitialize;

		// Reinitialize(world);

		// GridView.TileClicked += OnTileClicked;

		SetProcess(false);
		_client = new();
        var match = Regex.Match(address, @"(?<host>((\d+\.){3}\d+))\:(?<port>\d+)");
        if(!match.Success) throw new ArgumentException("Invalid ip address input");
        
        _host = match.Groups["host"].Value;
        _port = int.Parse(match.Groups["port"].Value);

		GD.Print($"Host: {_host}, Port: {_port}");

		_client.OnConnect += OnServerConenct;
		_client.OnMessage += OnServerMessage;
		_client.OnDisconnect += OnServerDisconnect;

		await _client.ConnectAsync(_host, _port);
	}

	public void Reinitialize(World world)
	{
		_context = new GameContext{
			World = world,
			CurrentTick = _loop.Tick,
			TimeStart = _context != null ? _context.TimeStart : Time.GetUnixTimeFromSystem(),
			LastTickProcessed = _loop.Tick
		};
		

		GridView.Initialize(_context);

		EntityManager.Initialize(_context);
	}

	
	public override void _Process(double delta)
	{
		// var now = Time.GetUnixTimeFromSystem();

		// while(now > _context.CalculateTickTime(_context.CurrentTick))
		// {
		// 	_loop.Update();

		// 	_context.CurrentTick++;
		// }
	}

	public void OnTileClicked(int x, int y)
	{
		GD.Print($"Tile was clicked at {new Int2(x, y)}");

		// _loop.InsertCommand(new MoveCommand(
		// 	0, _context.CurrentTick + 10, 0, new Int2(x, y)
		// ));
	}

    public override void _Input(InputEvent @event)
    {
        // if(@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Space)
		// {
		// 	_loop.RecoverState(0);
		// 	_context.TimeStart = Time.GetUnixTimeFromSystem();
		// }
    }

	public void OnServerConenct()
	{
		_client.SendAsync(Request<Login>.Generate(new Login("Anton")));
	}

	public async void OnServerMessage(byte[] bytes)
	{
		var responseType = (ResponseType) bytes[0];

		switch (responseType)
		{
			case ResponseType.PlayerData:
				var playerDataRes = new Response<Player>(bytes);
				GD.Print($"Welcome, player \"{playerDataRes.Content.Name}\" with ID {playerDataRes.Content.Id}");
				await _client.SendAsync(Request<Join>.Generate(new Join()));
				break;
			case ResponseType.WorldData:
				var worldDataRes = new Response<World>(bytes);
				World world = worldDataRes.Content;

				_loop = new Loop(world)
				{
				    Logger = new GDLogger()
				};

				_loop.WorldStateRecovered += Reinitialize;

				Reinitialize(world);

				GridView.TileClicked += OnTileClicked;

				GD.Print($"Joined a world with {world.Grid.Count} tiles and {world.Entities.Count} entities!");

				SetProcess(true);
				
				break;
		}
	}

	public void OnServerDisconnect()
	{
		
	}
}
