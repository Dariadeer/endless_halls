using Shared.Data;
using Shared.Logic;

namespace Client.Source.Data;

public class GameContext
{
    private static GameContext _instance;
    public static GameContext Instance
    {
        get
        {
            if (Instance is null)
            {
                _instance = new GameContext();
            }
            return _instance;
        }
    }

    public Camera Camera;
    public World World;
    public int PlayerTeam;

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
