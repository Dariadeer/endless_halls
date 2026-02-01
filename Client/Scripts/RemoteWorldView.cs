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
using System.Linq;

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
	private Entity _entity;

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
		_context.World.EntitySummoned += OnEntitySummoned;

		SetProcess(true);
	}

	public void OnWorldStateRecovered(World world)
	{
		_context.World = world;
		_context.CurrentTick = _loop.Tick;
		
		GridView.Initialize(_context);
		EntityManager.Initialize(_context);
		_context.World.EntitySummoned += OnEntitySummoned;
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

	public async void OnTileClicked(int x, int y)
	{
		GD.Print($"Tile was clicked at {new Int2(x, y)}");

		var move = new MoveCommand(-1, -1, _entity.Id, new Int2(x, y));

		await _client.SendAsync(ClientMessage<MoveCommand>.Generate(move));
		GD.Print("Sent move intent to the server!");
	}

    public override async void _Input(InputEvent @event)
    {
        if(@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
		{
			await _client.DisconnectAsync();
			Main.GoToMenu(this);
		}
    }

	public void OnEntitySummoned(Entity entity)
	{
		if(entity.TeamId == _player.Id)
		{
			_entity = entity;
		}
	}

	public void OnServerConnect()
	{
		_client.SendAsync(ClientMessage<Login>.Generate(new Login("Anton")));
	}

	public async void OnServerMessage(byte[] bytes)
	{
		GD.Print($"{bytes.Length} bytes received");
		var responseType = (ServerMessageType) bytes[0];
		GD.Print("Server Message Type: " + responseType);
		switch (responseType)
		{
			case ServerMessageType.PlayerData:
				var playerDataMsg = new ServerMessage<Player>(bytes);
				_player = playerDataMsg.Content;
				GD.Print($"Welcome, player \"{_player.Name}\" with ID {_player.Id}");
				await _client.SendAsync(ClientMessage<WorldDataRequest>.Generate(new WorldDataRequest()));
				break;
			case ServerMessageType.WorldState:
				var worldDataMsg = new ServerMessage<WorldState>(bytes);
				var worldData = worldDataMsg.Content;
				var world = worldData.World;
				try
				{
					_entity = world.Entities.First((KeyValuePair<int, Entity> pair) => pair.Value.TeamId == _player.Id).Value;
				} catch(Exception) {}

				LaunchWorldLoop(worldData);

				GD.Print($"Joined a world with {world.Grid.Count} tiles, {world.Entities.Count} entities and {worldData.Commands.Count} commands!");
				break;
			case ServerMessageType.Move:
				var moveCmdMsg = new ServerMessage<MoveCommand>(bytes);
				_loop.InsertCommand(moveCmdMsg.Content);
				break;
			case ServerMessageType.Summon:
				var summonCmdMsg = new ServerMessage<SummonCommand>(bytes);
				GD.Print("Summoning entity " + summonCmdMsg.Content.Summonee.Id);
				_loop.InsertCommand(summonCmdMsg.Content);
				break;
			default:
				GD.Print($"Couldn't recognize type of this message ({responseType})");
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
