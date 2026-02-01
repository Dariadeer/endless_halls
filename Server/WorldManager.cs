using Server.Utils;
using Shared.Data;
using Shared.Data.Commands;
using Shared.Logic;
using Shared.Network;
using Shared.Network.Messages;

namespace Server;

public class WorldManager
{
    public int randomCounter = 0;
    public int Id = 0;
    public Queue<ICommand> CommandQueue = [];
    public Dictionary<Connection, Player> Players = [];
    private Loop _loop;
    public int CommandTickDelay = 5;
    private readonly CancellationTokenSource _cts = new();

    public WorldManager(World world)
    {
        _loop = new Loop(world);
        _loop.Logger = new ServerLogger();
        _loop.SnapshotQuantity = 2;
    }

    public async Task StartLoop()
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromMilliseconds(Loop.TICK_DURATION_MS)
        );
        long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        while(await timer.WaitForNextTickAsync(_cts.Token))
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            while(now > start + _loop.Tick * Loop.TICK_DURATION_MS)
            {
                lock (CommandQueue)
                {
                    while(CommandQueue.Count > 0)
                    {
                        _loop.InsertCommand(CommandQueue.Dequeue());
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
        foreach(var connection in Players.Keys)
        {
            switch (command) {
                case MoveCommand move:
                    connection.SendAsync(ServerMessage<MoveCommand>.Generate(move));
                    break;
                case SummonCommand summon:
                    connection.SendAsync(ServerMessage<SummonCommand>.Generate(summon));
                    break;
            }
        }
    }

    public WorldState GetWorldData()
    {
        return _loop.GetWorldData();
    }

    public void AddPlayer(Connection connection, Player player) 
    {
        Players[connection] = player;
        var playerEntity = new Entity(randomCounter++)
        {
            Pos = new(0, 0),
            TeamId = player.Id,
            Movement = new Movement()
        };
        var serverSummon = new SummonCommand(1, GetDelayedTick(), playerEntity);
        _loop.InsertCommand(serverSummon);
        Broadcast(serverSummon);
    }

    public void RemovePlayer(Connection connection)
    {
        Players.Remove(connection);
    }

    public void ReceiveMovement(Connection connection, MoveCommand move)
    {
        var player = Players[connection];
        var entity = _loop.World.Entities[move.EntityId];
        if(player.Id == entity.TeamId)
        {
            var serverMove = new MoveCommand(2, GetDelayedTick(), entity.Id, move.To);
            _loop.InsertCommand(serverMove);
            Broadcast(serverMove);
        }
    }

    private int GetDelayedTick()
    {
        return _loop.Tick + CommandTickDelay;
    }

}