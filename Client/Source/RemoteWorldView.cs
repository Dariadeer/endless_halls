namespace Client.Scripts;

using System;
using Godot;
using Shared.Data;
using Shared.Logic;
using Shared.Data.Commands;
using Shared.MyMath;
using Shared.Utils;
using System.Text.RegularExpressions;
using Client.Scripts.Network;
using Shared.Network;
using Shared.Network.Messages;
using Client.Scripts.Data;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Client.Scripts.Utils.Abstracts;
using Client.Scripts.Utils.Interfaces;

public partial class RemoteWorldView : WorldView, ISceneNode
{
    private Loop _loop;
    [Export]
    public GridView GridView;
    [Export]
    public EntityManager EntityManager;
    [Export]
    public Camera Camera;

    private GameContext _context;
    private GameClient _client;
    private PingManager _pingManager = new();
    private ConcurrentQueue<ICommand> _commandBuffer = [];

    private Player _player;
    private Entity _entity;

    private string _host;
    private int _port;
    private long _worldDataRequestTime = 0;
    private long _currentDelay = 0;

    public static string ScenePath => "res://Scenes/RemoteWorld.tscn";

    public override void _Ready()
    {
        SetProcess(false);
    }
    public override async void Initialize(string[] args)
    {
        GlobalLogger.Instance.SetLogFunction(GD.Print);

        string address = args[0];

        _client = new();
        var match = Regex.Match(address, @"(?<host>((\d+\.){3}\d+))\:(?<port>\d+)");
        if (!match.Success) throw new ArgumentException("Invalid ip address input");

        _host = match.Groups["host"].Value;
        _port = int.Parse(match.Groups["port"].Value);

        GD.Print($"Host: {_host}, Port: {_port}");

        _client.OnConnect += OnServerConnect;
        _client.OnMessage += OnServerMessage;
        _client.OnDisconnect += OnServerDisconnect;

        await _client.ConnectAsync(_host, _port);
    }

    public void LaunchWorldLoop(WorldStateResponse data, long delay)
    {
        _loop = new Loop(data.World, data.SnapshotTick);

        _loop.InsertCommands(data.Commands);

        _context = new GameContext()
        {
            World = data.World,
            CurrentTick = data.SnapshotTick,
            TickStart = data.CurrentTick,
            TimeStart = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            Delay = delay,
            Camera = Camera
        };

        _loop.WorldStateRecovered += OnWorldStateRecovered;
        GridView.TileClicked += OnTileClicked;

        GridView.Initialize(_context);
        EntityManager.Initialize(_context);
        _context.World.EntityAppeared += OnEntitySummoned;

        CallDeferred("LaunchProcess");
    }

    public void LaunchProcess()
    {
        SetProcess(true);
    }

    public void HaltProcess()
    {
        SetProcess(false);
    }

    public void OnWorldStateRecovered(World world)
    {
        _context.World = world;
        _context.CurrentTick = _loop.Tick;

        GridView.Initialize(_context);
        EntityManager.Initialize(_context);
        _context.World.EntityAppeared += OnEntitySummoned;
    }


    public override void _Process(double delta)
    {
        var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        while (now > _context.CalculateTickTime(_context.CurrentTick))
        {
            _loop.Update();

            _context.CurrentTick++;
        }

        if (now - _pingManager.lastPingTime > PingManager.PING_INTERVAL)
        {
            _ = _client.SendAsync(ClientMessage<ClientPing>.Generate(
                _pingManager.Make(_loop.Tick)
            ));

            _pingManager.lastPingTime = now;
        }
    }

    public async void OnTileClicked(int x, int y)
    {
        GD.Print($"Tile was clicked at {new int2(x, y)} on tick {_loop.Tick}");

        var move = new MoveCommand(-1, _loop.Tick, _entity.Id, new int2(x, y));

        if (_loop.World.Grid[move.To].IsWalkable())
        {
            _ = _client.SendAsync(ClientMessage<MoveCommand>.Generate(move));
        }
    }

    public override async void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            switch (keyEvent.Keycode)
            {
                case Key.E:
                    var haltCommand = new HaltCommand(0, 0, _entity.Id);
                    _ = _client.SendAsync(ClientMessage<HaltCommand>.Generate(haltCommand));
                    break;
            }

        }
    }

    public void OnEntitySummoned(Entity entity)
    {
        if (entity.TeamId == _player.Id)
        {
            _entity = entity;
        }
    }

    public void OnServerConnect()
    {
        _client.SendAsync(ClientMessage<JoinRequest>.Generate(new JoinRequest("Anton")));
    }

    public void OnServerMessage(byte[] bytes)
    {
        try
        {
            // GD.Print($"{DateTimeOffset.Now.ToUnixTimeMilliseconds()} – message received");
            // GD.Print($"{bytes.Length} bytes received");
            var responseType = (ServerMessageType)bytes[0];
            if (responseType != ServerMessageType.Ping)
            {
                GD.Print("Server Message Type: " + responseType);
            }
            switch (responseType)
            {
                case ServerMessageType.PlayerData:
                    var playerDataMsg = new ServerMessage<Player>(bytes);
                    _player = playerDataMsg.Content;
                    Camera.EntityTeamIdFollowed = _player.Id;
                    GD.Print($"Welcome, player \"{_player.Name}\" with ID {_player.Id}");
                    _ = _client.SendAsync(ClientMessage<WorldDataRequest>.Generate(new WorldDataRequest()));
                    _worldDataRequestTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    break;
                case ServerMessageType.WorldState:
                    var delay = DateTimeOffset.Now.ToUnixTimeMilliseconds() - _worldDataRequestTime;
                    var worldDataMsg = new ServerMessage<WorldStateResponse>(bytes);
                    var worldData = worldDataMsg.Content;
                    var world = worldData.World;

                    LaunchWorldLoop(worldData, delay);

                    GD.Print($"Joined a world with {world.Grid.Count} tiles, {world.Entities.Count} entities and {worldData.Commands.Count} commands!");
                    break;
                case ServerMessageType.Movement:
                    var movement = new ServerMessage<MoveCommand>(bytes).Content;
                    _loop.InsertCommand(movement);
                    break;
                case ServerMessageType.Appearance:
                    var appearance = new ServerMessage<AppearCommand>(bytes).Content;
                    _loop.InsertCommand(appearance);
                    break;
                case ServerMessageType.Halt:
                    var halt = new ServerMessage<HaltCommand>(bytes).Content;
                    _loop.InsertCommand(halt);
                    break;
                case ServerMessageType.Ping:
                    var ping = new ServerMessage<ServerPing>(bytes).Content;
                    _currentDelay = _pingManager.GetDelay(ping);
                    // GD.Print($"Ping: {_pingManager.GetDelay(ping)}");
                    break;
                case ServerMessageType.Disappearance:
                    var disappearance = new ServerMessage<DisappearCommand>(bytes).Content;
                    _loop.InsertCommand(disappearance);
                    break;
                default:
                    GD.Print($"Couldn't recognize type of this message ({responseType})");
                    break;
            }
        }
        catch (Exception e)
        {
            GD.Print(e);
        }
    }

    public void OnServerDisconnect()
    {
        GD.Print("Server disconnected!");
    }

    public override Task Terminate()
    {
        return _client.DisconnectAsync();
    }
}
