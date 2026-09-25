using Shared.Data;
using Shared.Logic;

namespace Client.Scripts.Data;

public class GameContext
{
    public Camera Camera;
    public World World;
    public int CurrentTick;
    public double LastTickProcessed;
    public long TimeStart;
    public int TickStart = 0;
    public long Delay = 0;

    public long CalculateTickTime(int tick)
    {
        return Loop.TICK_DURATION_MS * (tick - TickStart) + TimeStart + Delay;
    }
}