using Server.Utils;
using Shared.Data;
using Shared.Data.Commands;
using Shared.Logic;

namespace Server;

public class WorldManager
{
    public Queue<ICommand> commandQueue = [];
    private Loop _loop;
    private readonly CancellationTokenSource _cts = new();

    public WorldManager(World world)
    {
        _loop = new Loop(world);
        _loop.Logger = new ServerLogger();
        _loop.SnapshotQuantity = 1;
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
                lock (commandQueue)
                {
                    while(commandQueue.Count > 0)
                    {
                        _loop.InsertCommand(commandQueue.Dequeue());
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

    public World GetWorldData()
    {
        return _loop.GetLastSnapshot();
    }
}