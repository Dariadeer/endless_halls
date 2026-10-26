using Shared.Data;
using Shared.Data.Vision;
using Shared.MyMath;

namespace Tests;

public class BreadthFirstSearchVisionProviderTest
{
    [Fact]
    public void DefaultTileMapVisionUsesBreadthFirstSearchAndStopsAtWalls()
    {
        var map = new TileMap();
        map.AddTile(new Tile(new int2(0, 0)));
        map.AddTile(new Tile(new int2(0, 1), 1));
        map.AddTile(new Tile(new int2(0, 2)));

        Assert.IsType<BreadthFirstSearchVisionProvider>(map.VisionProvider);

        var visible = map.GetVisionArea(int2.Zero);

        Assert.Contains(visible, tile => tile.Pos == new int2(0, 1));
        Assert.DoesNotContain(visible, tile => tile.Pos == new int2(0, 2));
    }

    [Fact]
    public void VisionRangeIsSixTiles()
    {
        var map = new TileMap();
        for (int x = 0; x <= BreadthFirstSearchVisionProvider.DefaultVisionRange + 2; x++)
        {
            map.AddTile(new Tile(new int2(x, 0)));
        }

        var visible = map.GetVisionArea(int2.Zero);

        Assert.Contains(visible, tile => tile.Pos == new int2(6, 0));
        Assert.DoesNotContain(visible, tile => tile.Pos == new int2(7, 0));
    }
}
