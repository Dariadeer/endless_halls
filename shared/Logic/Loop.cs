namespace Shared.Logic;

using Shared.Data;
using Shared.Logic.Commands;

public class Loop
{
    public readonly World _world;
    public readonly int Tick;
    private readonly Dictionary<int, PriorityQueue<ICommand, int>> _commands = new();

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

    public void Update()
    {
        
    }
}