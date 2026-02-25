namespace Shared.Logic;

using Shared.Data;
using Shared.Data.Commands;
using Shared.MyMath;
using Shared.Network;
using Shared.Utils;

public class Loop
{
    public static readonly long TICK_DURATION_MS = 25;
    public static readonly double TICK_DURATION_S = 0.025;

    public int SnapshotInterval = 40;
    public int SnapshotQuantity = 6;
    public event Action<World> WorldStateRecovered;

	public ILogger? Logger;
    public World World;
    public int Tick;
    public int FurthestTickProcessed = 0;
    private readonly Dictionary<int, CommandList> _commands = [];
    private int _lastCommandTick = 0;
    private readonly Dictionary<int, World> snapshots = [];

    public Loop(World world)
    {
        Tick = 0;
        World = world;
        snapshots[Tick] = World.Copy();
    }

    public Loop(World world, int initialTick)
    {
        Tick = initialTick;
        World = world;
        snapshots[Tick] = World.Copy();
    }

    public void Update(int tick)
    {
        AdvanceWorld(tick);

        if((tick + 1) % SnapshotInterval == 0)
        {
            snapshots[tick + 1] = World.Copy();

            int snapshotToRemoveTick = tick + 1 - SnapshotQuantity * SnapshotInterval;
            if(snapshotToRemoveTick != 0)
            {
                snapshots.Remove(snapshotToRemoveTick);
            }

            // Logger?.Log("Snapshot created at " + (tick + 1) + ", total of " + snapshots.Count);
        }
    }

    public void Update()
    {
        Update(Tick);

        if(Tick > FurthestTickProcessed)
        {
            FurthestTickProcessed = Tick;
        }

        // Logger?.Log(Tick);

        Tick++;
    }

    public void AdvanceWorld(int tick)
    {   
        World.Advance(tick);

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

        World = snapshot;

        Logger?.Log($"Recovering from snapshot {currentTick}/{Tick} ({tick})");

        Tick = currentTick;

        WorldStateRecovered?.Invoke(World);
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
            _commands.Add(tick, []);
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
        Logger?.Log("Applying " + command + " " + command.Id + " at tick " + Tick);
        Entity? entity;
        switch (command)
        {
            case MoveCommand move:
                entity = FetchEntity(move.EntityId);
                if(entity == null) break;
                var lastPos = entity.Path.LastOrDefault(entity.Movement.State == MovementState.Idle ? entity.Pos : entity.Movement.To);
                var steps = World.Pathfinder.AStar(lastPos, move.To);
                entity.AppendPath(steps, Tick);
                Logger?.Log($"{steps.Count} tiles to travel, {entity.Path.Count} steps in total!");
                break;
            case AppearCommand appear:
                entity = appear.Entity.Copy();
                World.SummonEntity(entity);
                break;
            case HaltCommand halt:
                entity = FetchEntity(halt.EntityId);
                if(entity == null) break;
                entity.ClearPath();
                break;
            case DisappearCommand disappear:
                entity = FetchEntity(disappear.EntityId);
                if(entity == null) break;
                World.RemoveEntity(entity);
                break;
            default:
                Logger?.Log("Unrecognized command!");
                break;
        }
    }

    public Entity? FetchEntity(int entityId)
    {
        if(World.Entities.TryGetValue(entityId, out var entity))
        {
            return entity;
        } else
        {
            return null;
        }
    }

    public WorldStateResponse GetWorldData()
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
        return new WorldStateResponse(Tick, snapshotTick, snapshots[snapshotTick], commands);
    }
}