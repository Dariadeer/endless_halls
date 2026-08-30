using System.Collections.Concurrent;
using Server.Utils;
using Shared.Data;
using Shared.Data.Commands;
using Shared.Logic;
using Shared.Network;
using Shared.Network.Messages;

namespace Server;

public class WorldManager
{
    private int _randomCounter;
    private int _nextCommandId = 0;
    public ConcurrentQueue<ICommand> CommandQueue = [];
    public Dictionary<Connection, Player> Players = [];
    private Loop _loop;
    public int CommandTickDelay = 2;
    private readonly CancellationTokenSource _cts = new();

    public WorldManager(World world)
    {
        _loop = new Loop(world)
        {
            Logger = new ServerLogger(),
            SnapshotQuantity = 2
        };
    }

    private int NextCommandId
    {
        get => _nextCommandId++;
    }

    public async Task StartLoop()
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromMilliseconds(Loop.TICK_DURATION_MS)
        );
        long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        while (await timer.WaitForNextTickAsync(_cts.Token))
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            while (now > start + _loop.Tick * Loop.TICK_DURATION_MS)
            {
                lock (CommandQueue)
                {
                    while (CommandQueue.TryDequeue(out var command))
                    {
                        _loop.InsertCommand(command);
                        Broadcast(command);
                    }
                    _loop.Update();
                }
            }
        }
    }

    public void EndLoop()
    {
        _cts.Cancel();
    }

    public void Broadcast(ICommand command)
    {
        foreach (var connection in Players.Keys)
        {
            switch (command)
            {
                case MoveCommand move:
                    _ = connection.SendAsync(ServerMessage<MoveCommand>.Generate(move));
                    break;
                case AppearCommand appear:
                    if (Players[connection].Id != appear.Id)
                    {
                        _ = connection.SendAsync(ServerMessage<AppearCommand>.Generate(appear));
                    }
                    break;
                case HaltCommand halt:
                    _ = connection.SendAsync(ServerMessage<HaltCommand>.Generate(halt));
                    break;
                case DisappearCommand disappear:
                    _ = connection.SendAsync(ServerMessage<DisappearCommand>.Generate(disappear));
                    break;
            }
        }
    }

    public WorldStateResponse GetWorldData()
    {
        return _loop.GetWorldData();
    }

    public void AddPlayer(Connection connection, Player player)
    {
        Players[connection] = player;
        var playerEntity = new Entity(_randomCounter++)
        {
            Pos = new(0, 0),
            TeamId = player.Id,
            Movement = new Movement(),
        };
        var serverSummon = new AppearCommand(NextCommandId, GetDelayedTick(), playerEntity);
        CommandQueue.Enqueue(serverSummon);
    }

    public void RemovePlayer(Connection connection)
    {
        var player = Players[connection];
        Players.Remove(connection);
        foreach (var entity in _loop.World.Entities.Values)
        {
            if (entity.TeamId != player.Id) continue;
            var disappearance = new DisappearCommand(NextCommandId, GetDelayedTick(), entity.Id);
            CommandQueue.Enqueue(disappearance);
        }
    }

    public void ProcessMovement(Connection connection, MoveCommand move)
    {
        var player = Players[connection];
        var entity = _loop.World.Entities[move.EntityId];
        if (player.Id != entity.TeamId) return;
        var serverMove = new MoveCommand(NextCommandId, GetDelayedTick(), entity.Id, move.To);
        CommandQueue.Enqueue(serverMove);
    }

    public void ProcessHalt(Connection conenction, HaltCommand halt)
    {
        var player = Players[conenction];
        var entity = _loop.World.Entities[halt.EntityId];
        if (player.Id != entity.TeamId) return;
        var serverHalt = new HaltCommand(NextCommandId, GetDelayedTick(), entity.Id);
        CommandQueue.Enqueue(serverHalt);
    }

    private int GetDelayedTick()
    {
        return _loop.Tick + CommandTickDelay;
    }

}
