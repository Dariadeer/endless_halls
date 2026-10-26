using Shared.MyMath;

namespace Shared.Data.Vision;

public sealed class DirectionBasedVisionProvider : IVisionProvider
{
    public const int VisionRayJumpCount = 3;

    private static readonly int2[] NeighborOffsets =
    [
        int2.Right,
        int2.Down,
        new int2(-1, -1),
        int2.Left,
        int2.Up,
        new int2(1, 1),
    ];

    private readonly record struct VisionRay(
        int2 Direction,
        int2 CheckOffsetA,
        int2 CheckOffsetB,
        int2[]? HaloOffsetsA,
        int2[]? HaloOffsetsB);

    private static readonly VisionRay[] VisionRays =
    [
        // Primary rays check the tile directly between this step and the previous one.
        new(new int2(2, 0), new int2(-1, 0), int2.Zero, null, null),
        new(new int2(0, 2), new int2(0, -1), int2.Zero, null, null),
        new(new int2(2, 2), new int2(-1, -1), int2.Zero, null, null),
        new(new int2(-2, 0), new int2(1, 0), int2.Zero, null, null),
        new(new int2(0, -2), new int2(0, 1), int2.Zero, null, null),
        new(new int2(-2, -2), new int2(1, 1), int2.Zero, null, null),

        // Each open upstream tile reveals its corresponding half of the ray halo.
        new(new int2(2, 1), new int2(-1, 0), new int2(-1, -1), [new int2(0, 1), new int2(1, 1)], [new int2(0, -1), new int2(1, 0)]),
        new(new int2(1, 2), new int2(0, -1), new int2(-1, -1), [new int2(-1, 0), new int2(0, 1)], [new int2(-1, -1), new int2(1, 0)]),
        new(new int2(-1, 1), new int2(1, 0), new int2(0, -1), [new int2(0, 1), new int2(1, 1)], [new int2(-1, -1), new int2(0, -1)]),
        new(new int2(-2, -1), new int2(1, 0), new int2(1, 1), [new int2(0, -1), new int2(-1, -1)], [new int2(0, 1), new int2(-1, 0)]),
        new(new int2(-1, -2), new int2(0, 1), new int2(1, 1), [new int2(1, 0), new int2(0, -1)], [new int2(1, 1), new int2(-1, 0)]),
        new(new int2(1, -1), new int2(0, -1), new int2(1, 0), [new int2(1, 1), new int2(0, 1)], [new int2(-1, -1), new int2(0, -1)]),
    ];

    public List<Tile> GetVisionArea(TileMap map, int2 origin)
    {
        var visible = new HashSet<int2>();
        var primarySeeds = new HashSet<int2>();
        var haloSources = new Dictionary<int2, HashSet<int2>>();
        var wallShadowExemptSources = new Dictionary<int2, HashSet<int2>>();

        if (map.TryGetValue(origin, out var originTile) && originTile.IsWalkable())
        {
            primarySeeds.Add(origin);
            visible.Add(origin);
        }

        foreach (var ray in VisionRays)
        {
            for (int jump = 1; jump <= VisionRayJumpCount; jump++)
            {
                var pos = origin + jump * ray.Direction;
                if (!map.TryGetValue(pos, out var tile) || !tile.IsWalkable())
                {
                    break;
                }

                bool checkAOpen = IsWalkable(map, pos + ray.CheckOffsetA);
                bool checkBOpen = ray.HaloOffsetsB is null || IsWalkable(map, pos + ray.CheckOffsetB);
                bool canContinue = ray.HaloOffsetsB is null
                    ? checkAOpen
                    : checkAOpen || checkBOpen;
                if (!canContinue)
                {
                    break;
                }

                if (ray.HaloOffsetsB is null)
                {
                    primarySeeds.Add(pos);
                }
                else
                {
                    visible.Add(pos);
                    AddHaloHalf(pos, ray.HaloOffsetsA, checkAOpen);
                    AddHaloHalf(pos, ray.HaloOffsetsB, checkBOpen);
                }
            }
        }

        // Primary rays reveal one open neighboring tile around each seed.
        visible.UnionWith(primarySeeds);
        foreach (var pos in primarySeeds)
        {
            foreach (var offset in NeighborOffsets)
            {
                var neighborPos = pos + offset;
                if (IsWalkable(map, neighborPos))
                {
                    visible.Add(neighborPos);
                }
            }
        }

        // Walls do not propagate vision, but a wall bordering visible ground is visible.
        var visibleWalls = new HashSet<int2>();
        foreach (var pos in visible)
        {
            foreach (var offset in NeighborOffsets)
            {
                var neighborPos = pos + offset;
                if (map.TryGetValue(neighborPos, out var neighbor) && !neighbor.IsWalkable())
                {
                    visibleWalls.Add(neighborPos);
                }
            }
        }
        visible.UnionWith(visibleWalls);

        // A wall blocks halo contributions from seeds behind it, while another seed
        // can still reveal the tile around the wall.
        var shadowedTiles = visible
            .Where(pos => IsWalkable(map, pos)
                && haloSources.TryGetValue(pos, out var sources)
                && sources.All(source =>
                    (!wallShadowExemptSources.TryGetValue(pos, out var exemptSources)
                        || !exemptSources.Contains(source))
                    && NeighborOffsets.Any(offset =>
                    {
                        var wallPos = pos + offset;
                        return visibleWalls.Contains(wallPos)
                            && origin.Distance(wallPos) + wallPos.Distance(pos) == origin.Distance(pos)
                            && origin.Distance(wallPos) + wallPos.Distance(source) == origin.Distance(source);
                    })))
            .ToList();
        visible.ExceptWith(shadowedTiles);

        return visible.Select(pos => map[pos]).ToList();

        void AddHaloHalf(int2 center, int2[]? offsets, bool checkIsOpen)
        {
            if (offsets is null)
            {
                return;
            }

            // An open check reveals both halo tiles. A wall blocks the nearer one,
            // while the furthest tile in that half remains visible.
            var firstIndex = checkIsOpen ? 0 : offsets.Length - 1;
            var lastIndex = offsets.Length - 1;
            for (int i = firstIndex; i <= lastIndex; i++)
            {
                var haloPos = center + offsets[i];
                if (!map.ContainsKey(haloPos))
                {
                    continue;
                }

                visible.Add(haloPos);
                AddHaloSource(haloPos, center);
                if (!checkIsOpen && !HasDirectWallBetween(origin, haloPos))
                {
                    AddWallShadowExemptSource(haloPos, center);
                }
            }
        }

        bool HasDirectWallBetween(int2 from, int2 to)
        {
            return NeighborOffsets.Any(offset =>
            {
                var wallPos = to + offset;
                return map.TryGetValue(wallPos, out var wall)
                    && !wall.IsWalkable()
                    && IsDirectlyBehind(from, wallPos, to);
            });
        }

        void AddHaloSource(int2 haloPos, int2 source)
        {
            if (!haloSources.TryGetValue(haloPos, out var sources))
            {
                sources = [];
                haloSources.Add(haloPos, sources);
            }

            sources.Add(source);
        }

        void AddWallShadowExemptSource(int2 haloPos, int2 source)
        {
            if (!wallShadowExemptSources.TryGetValue(haloPos, out var sources))
            {
                sources = [];
                wallShadowExemptSources.Add(haloPos, sources);
            }

            sources.Add(source);
        }
    }

    private static bool IsWalkable(TileMap map, int2 pos)
    {
        return map.TryGetValue(pos, out var tile) && tile.IsWalkable();
    }

    private static bool IsDirectlyBehind(int2 origin, int2 wall, int2 pos)
    {
        long wallX = wall.X - origin.X;
        long wallY = wall.Y - origin.Y;
        long posX = pos.X - origin.X;
        long posY = pos.Y - origin.Y;

        return wallX * posY == wallY * posX
            && wallX * posX + wallY * posY > 0
            && wallX * wallX + wallY * wallY < posX * posX + posY * posY;
    }
}
