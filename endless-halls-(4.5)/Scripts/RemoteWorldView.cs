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
	public Main? Main;

	private GameContext _context;
	private GameClient _client;
    private string _host;
    private int _port;
	private Player _player;

    public override void _Ready()
    {
		SetProcess(false);
    }
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

		_client = new();
        var match = Regex.Match(address, @"(?<host>((\d+\.){3}\d+))\:(?<port>\d+)");
        if(!match.Success) throw new ArgumentException("Invalid ip address input");
        
        _host = match.Groups["host"].Value;
        _port = int.Parse(match.Groups["port"].Value);

		GD.Print($"Host: {_host}, Port: {_port}");

		_client.OnConnect += OnServerConnect;
		_client.OnMessage += OnServerMessage;
		_client.OnDisconnect += OnServerDisconnect;

		await _client.ConnectAsync(_host, _port);
	}

	public void LaunchWorldLoop(WorldState data)
	{
		_loop = new Loop(data.World, data.SnapshotTick)
        {
            Logger = new GDLogger()
        };

		_loop.InsertCommands(data.Commands);

		_context = new GameContext()
		{
			World = data.World,
			CurrentTick = data.SnapshotTick,
			TickStart = data.CurrentTick,
			TimeStart = Time.GetUnixTimeFromSystem()
		};

		_loop.WorldStateRecovered += OnWorldStateRecovered;
		GridView.TileClicked += OnTileClicked;

		GridView.Initialize(_context);
		EntityManager.Initialize(_context);

		SetProcess(true);
	}

	public void OnWorldStateRecovered(World world)
	{
		_context.World = world;
		_context.CurrentTick = _loop.Tick;
		
		GridView.Initialize(_context);
		EntityManager.Initialize(_context);
	}

	
	public override void _Process(double delta)
	{
		var now = Time.GetUnixTimeFromSystem();;

		while(now > _context.CalculateTickTime(_context.CurrentTick))
		{
			_loop.Update();

			_context.CurrentTick++;
		}
	}

	public void OnTileClicked(int x, int y)
	{
		GD.Print($"Tile was clicked at {new Int2(x, y)}");

		// _loop.InsertCommand(new MoveCommand(
		// 	0, _context.CurrentTick + 10, 0, new Int2(x, y)
		// ));
	}

    public override async void _Input(InputEvent @event)
    {
        if(@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
		{
			await _client.DisconnectAsync();
			Main.GoToMenu(this);
		}
    }

	public void OnServerConnect()
	{
		_client.SendAsync(ClientMessage<Login>.Generate(new Login("Anton")));
	}

	public async void OnServerMessage(byte[] bytes)
	{
		var responseType = (ServerMessageType) bytes[0];
		switch (responseType)
		{
			case ServerMessageType.PlayerData:
				var playerDataRes = new ServerMessage<Player>(bytes);
				GD.Print($"Welcome, player \"{playerDataRes.Content.Name}\" with ID {playerDataRes.Content.Id}");
				await _client.SendAsync(ClientMessage<Join>.Generate(new Join()));
				break;
			case ServerMessageType.WorldState:
				var worldDataRes = new ServerMessage<WorldState>(bytes);
				var worldData = worldDataRes.Content;
				var world = worldData.World;

				LaunchWorldLoop(worldData);

				GD.Print($"Joined a world with {world.Grid.Count} tiles, {world.Entities.Count} entities and {worldData.Commands.Count} commands!");
				break;
		}
	}

	public void OnServerDisconnect()
	{
		SetProcess(false);
		GD.Print("Server disconnected!");
		if(Main != null)
		{
			Main.GoToMenu(this);
		}
	}
}
