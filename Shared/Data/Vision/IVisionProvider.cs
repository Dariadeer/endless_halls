using Shared.MyMath;

namespace Shared.Data.Vision;

public interface IVisionProvider
{
    List<Tile> GetVisionArea(TileMap map, int2 origin);
}
