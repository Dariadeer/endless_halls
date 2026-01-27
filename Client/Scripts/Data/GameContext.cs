using Shared.Data;
using Shared.Logic;

namespace Client.Scripts;

public class GameContext
{
    public World World;
    public int CurrentTick;
    public double LastTickProcessed;
    public double TimeStart;

    public double CalculateTickTime(int tick)
    {
        return Loop.TICK_DURATION_S * tick + TimeStart;
    }
}