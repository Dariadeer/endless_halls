namespace Shared.Logic;

using Shared.Data;
using Shared.Data.Commands;
using Shared.Utils;

public class Loop
{
    public static readonly int TICK_DURATION_MS = 20;
    public static readonly double TICK_DURATION_S = 0.02;

    public int SnapshotInterval = 50;
    public int SnapshotQuantity = 6;
    public event Action<World> WorldStateRecovered;

	public ILogger? Logger;
    private World _world;
    public int Tick;
    private readonly Dictionary<int, CommandList> _commands = [];
    private readonly Dictionary<int, World> snapshots = [];

    public Loop(World world)
    {
        Tick = 0;
        _world = world;
    }

    public Loop(World world, int initialTick)
    {
        Tick = initialTick;
        _world = world;
    }

    public void Update(int tick)
    {
        if(tick % SnapshotInterval == 0)
        {
            snapshots[tick] = _world.Copy();

            int snapshotToRemoveTick = tick - SnapshotQuantity * SnapshotInterval;
            if(snapshotToRemoveTick != 0)
            {
                snapshots.Remove(snapshotToRemoveTick);
            }

            // Logger?.Log("Snapshot created at " + tick + ", total of " + snapshots.Count);
        }

        AdvanceWorld(tick);
    }

    public void Update()
    {
        Update(Tick);

        Tick++;
    }

    public void AdvanceWorld(int tick)
    {
        if (_commands.ContainsKey(tick))
        {
            foreach(var command in _commands[tick].GetAll()) {
                ApplyCommand(command);
            }
        }

        
        foreach(var entity in _world.Entities.Values)
        {
            var movement = entity.Movement;
            if(movement.State == MovementState.Moving && movement.End == tick)
            {
                entity.Pos = movement.To;
                entity.Movement = new Movement();
            }
        }
    }

    public void RecoverState(int tick)
    {
        var currentTick = tick / SnapshotInterval * SnapshotInterval;
        var snapshot = snapshots[currentTick];

        _world = snapshot;

        Logger?.Log($"Recovering from snapshot {currentTick}/{Tick} ({tick})");

        Tick = currentTick;

        WorldStateRecovered.Invoke(_world);
    }

    public void InsertCommand(ICommand command)
    {
        int tick = command.Tick;
        var recoveryNeeded = false;
        var recoveryRange = (SnapshotQuantity - 1) * SnapshotInterval;
        if(tick < Tick)
        {
            if(tick >= Tick - recoveryRange && tick >= 0)
            {
                recoveryNeeded = true;
            } 
            else
            {
                throw new ArgumentException($"Cannot recover state {Tick - tick} ticks behind, maximum is {(SnapshotQuantity - 1) * SnapshotInterval}");
            }
        }

        if(!_commands.ContainsKey(tick))
        {
            _commands.Add(tick, new CommandList());
        }

        var commandsAtTick = _commands[tick];

        commandsAtTick.Add(command);

        if(recoveryNeeded) RecoverState(tick);
    }

    public void ApplyCommand(ICommand command)
    {
        Logger?.Log("Applying Command " + command.Id + " at tick " + Tick);
        Entity entity;
        switch (command)
        {
            case MoveCommand move:
                entity = _world.Entities[move.EntityId];
                entity.Movement = new Movement(
                    Tick, Tick + 50, move.To
                );
                break;
            case SummonCommand summon:
                entity = summon.Summonee.Copy();
                _world.SummonEntity(entity);
                break;
        }
    }

    public World GetLastSnapshot()
    {
        var snapshotTick = Tick / SnapshotInterval * SnapshotInterval;
        return snapshots[snapshotTick];
    }
}