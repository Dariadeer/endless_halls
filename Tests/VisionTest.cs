using Shared.Data;
using Shared.Data.Vision;
using Shared.MyMath;

namespace Tests;

public class VisionTest
{
    [Fact]
    public void WallDirectlyAheadBlocksTileBehindIt()
    {
        var map = new TileMap(new DirectionBasedVisionProvider());
        var wallPosition = new int2(0, 1);

        for (int x = -8; x <= 8; x++)
        {
            for (int y = -8; y <= 8; y++)
            {
                var position = new int2(x, y);
                map.AddTile(new Tile(position, position == wallPosition ? (byte)1 : (byte)0));
            }
        }

        var visible = map.GetVisionArea(new int2(0, 0));

        Assert.Contains(visible, tile => tile.Pos == wallPosition);
        Assert.DoesNotContain(visible, tile => tile.Pos == new int2(0, 2));
    }

    [Fact]
    public void SecondaryRayHaloDoesNotRevealTileBehindWallOnSeededMap()
    {
        const int mapSeed = 1;
        var map = new TileMap(new DirectionBasedVisionProvider());
        var generator = new TileMapGenerator(mapSeed);

        for (int x = -8; x <= 8; x++)
        {
            for (int y = -8; y <= 8; y++)
            {
                var position = new int2(x, y);
                map.AddTile(generator.GenerateTile(position));
            }
        }

        var wallPosition = new int2(-2, -2);
        var positionBehindWall = new int2(-2, -1);
        Assert.False(map[wallPosition].IsWalkable());

        var visible = map.GetVisionArea(new int2(-2, -3));

        Assert.DoesNotContain(visible, tile => tile.Pos == positionBehindWall);
    }

    [Fact]
    public void WallDoesNotBlockHaloFromAnotherRaySeed()
    {
        var map = new TileMap(new DirectionBasedVisionProvider());
        var generator = new TileMapGenerator(1);

        for (int x = -8; x <= 8; x++)
        {
            for (int y = -8; y <= 8; y++)
            {
                var position = new int2(x, y);
                map.AddTile(generator.GenerateTile(position));
            }
        }

        var visible = map.GetVisionArea(int2.Zero);

        Assert.Contains(visible, tile => tile.Pos == new int2(-1, 1));
    }

    [Fact]
    public void SecondaryRayHaloRevealsExpectedTilesOnSeededMap()
    {
        var map = new TileMap(new DirectionBasedVisionProvider());
        var generator = new TileMapGenerator(1);

        for (int x = -8; x <= 8; x++)
        {
            for (int y = -8; y <= 8; y++)
            {
                var position = new int2(x, y);
                map.AddTile(generator.GenerateTile(position));
            }
        }

        var visible = map.GetVisionArea(int2.Zero);

        Assert.Contains(visible, tile => tile.Pos == new int2(-2, 3));
        Assert.Contains(visible, tile => tile.Pos == new int2(-1, 3));
    }

    [Fact]
    public void CurrentVisionAtSameDestinationConvergesRegardlessOfStartingPosition()
    {
        var destination = new int2(3, -12);
        var tileToCheck = new int2(1, -11);

        Assert.False(IsCurrentlyVisibleAfterMove(new int2(3, -13), destination, tileToCheck));
        Assert.False(IsCurrentlyVisibleAfterMove(new int2(3, -11), destination, tileToCheck));
    }

    [Fact]
    public void BlockedSecondaryHaloKeepsItsFurthestTileVisible()
    {
        var map = new TileMap(new DirectionBasedVisionProvider());
        var generator = new TileMapGenerator();
        for (int x = -8; x <= 8; x++)
        {
            for (int y = -8; y <= 8; y++)
            {
                var position = new int2(x, y);
                map.AddTile(generator.GenerateTile(position));
            }
        }

        Assert.False(map[new int2(-1, 0)].IsWalkable());

        var visible = map.GetVisionArea(int2.Zero);

        Assert.DoesNotContain(visible, tile => tile.Pos == new int2(-2, 0));
        Assert.Contains(visible, tile => tile.Pos == new int2(-3, -1));
    }

    private static bool IsCurrentlyVisibleAfterMove(int2 start, int2 destination, int2 tileToCheck)
    {
        var map = new TileMap(new DirectionBasedVisionProvider());
        var generator = new TileMapGenerator(1);
        for (int x = -24; x <= 24; x++)
        {
            for (int y = -24; y <= 24; y++)
            {
                var position = new int2(x, y);
                map.AddTile(generator.GenerateTile(position));
            }
        }

        var entity = new Entity(1) { TeamId = 0, Pos = start };
        var entities = new EntityMap();
        entities.AddEntity(entity);
        var world = new World(map, entities);

        entity.Movement = new Movement(0, 1, destination);
        for (int tick = 1; tick <= 500; tick++)
        {
            world.Advance(tick);
        }

        return world.IsTileInCurrentVisionForTeam(entity.TeamId, tileToCheck);
    }
}
