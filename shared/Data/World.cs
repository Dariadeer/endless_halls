namespace Shared.Data;

public class World
{
    public readonly HexGrid Grid;

    public World(HexGrid grid)
    {
        Grid = grid;
    }
}