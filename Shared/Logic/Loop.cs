namespace Shared.Logic;

using Shared.Data;
using Shared.Data.Commands;
using Shared.Network;
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
    public int FurthestTickProcessed = 0;
    private readonly Dictionary<int, CommandList> _commands = [];
    private int _lastCommandTick = 0;
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

        if(Tick > FurthestTickProcessed)
        {
            FurthestTickProcessed = Tick;
        }

        Tick++;
    }

    public void AdvanceWorld(int tick)
    {   
        foreach(var entity in _world.Entities.Values)
        {
            var movement = entity.Movement;
            if(movement.State == MovementState.Moving && movement.End == tick)
            {
                entity.Pos = movement.To;
                entity.Movement = new Movement();
                Logger?.Log($"Entity {entity.Id} has arrived at tile {entity.Pos}");
            }
        }

        if (_commands.ContainsKey(tick))
        {
            foreach(var command in _commands[tick]) {
                ApplyCommand(command);
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
            if(tick >= FurthestTickProcessed - recoveryRange && tick >= 0)
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
        
        if(tick > _lastCommandTick)
        {
            _lastCommandTick = tick;
        }

        if(recoveryNeeded) RecoverState(tick);
    }

    public void InsertCommands(CommandList commands)
    {
        // Verify commands

        var recoveryNeeded = false;
        int leastRecentTick = 0;
        var recoveryRange = (SnapshotQuantity - 1) * SnapshotInterval;
        foreach (var command in commands)
        {
            int tick = command.Tick;
            if(tick < leastRecentTick)
            {
                leastRecentTick = tick;
            }
            if(tick < Tick)
            {
                if(tick >= FurthestTickProcessed - recoveryRange && tick >= 0)
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
                _commands.Add(tick, []);
            }
            var commandsAtTick = _commands[tick];
            commandsAtTick.Add(command);
        }

        if(recoveryNeeded) RecoverState(leastRecentTick);
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

    public WorldState GetWorldData()
    {
        var snapshotTick = Tick / SnapshotInterval * SnapshotInterval;
        var commands = new CommandList();
        for(int tick = snapshotTick; tick <= _lastCommandTick; tick++)
        {
            if(_commands.ContainsKey(tick))
            {
                foreach (var command in _commands[tick])
                {
                    commands.Add(command);
                }
            }
        }
        return new WorldState(Tick, snapshotTick, snapshots[snapshotTick], commands);
    }
}