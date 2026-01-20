using Shared.Math;

namespace Shared.Data;
public class HexGrid
{
    private Dictionary<int, HexTile> _tiles = new();

    public void Generate(int radius) 
    {
        for(int x = -radius + 1; x < radius; x++) {
            for(int y = -radius + 1; y < radius; y++) {
                if((x > 0 && y > 0) || (x < 0 && y < 0) || MathF.Abs(x) + MathF.Abs(y) < radius) {
                    Add(
                        new HexTile(new Int2(x, y))
                    );
                }
                
            }
        }
    }
    
    public void Add(HexTile tile)
    {
        _tiles.Add(tile.Pos.Pack(), tile);
    }

    public HexTile Get(Int2 pos)
    {
        return _tiles[pos.Pack()];
    }

    public IEnumerable<HexTile> GetAll()
    {
        return _tiles.Values.AsEnumerable();
    }
}