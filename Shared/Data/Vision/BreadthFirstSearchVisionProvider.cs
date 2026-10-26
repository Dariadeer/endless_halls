using Shared.MyMath;

namespace Shared.Data.Vision;

public sealed class BreadthFirstSearchVisionProvider(
    int visionRange = BreadthFirstSearchVisionProvider.DefaultVisionRange) : IVisionProvider
{
    public const int DefaultVisionRange = 6;

    private static readonly int2[] NeighborOffsets =
    [
        int2.Right,
        int2.Down,
        new int2(-1, -1),
        int2.Left,
        int2.Up,
        new int2(1, 1),
    ];

    public List<Tile> GetVisionArea(TileMap map, int2 origin)
    {
        if (visionRange < 0)
        {
            throw new InvalidOperationException("Vision range cannot be negative.");
        }

        if (!map.TryGetValue(origin, out var originTile) || !originTile.IsWalkable())
        {
            return [];
        }

        var visible = new HashSet<int2> { origin };
        var distanceByPosition = new Dictionary<int2, int> { [origin] = 0 };
        var queue = new Queue<int2>();
        queue.Enqueue(origin);

        while (queue.TryDequeue(out var position))
        {
            int distance = distanceByPosition[position];
            if (distance >= visionRange)
            {
                continue;
            }

            foreach (var offset in NeighborOffsets)
            {
                var neighborPos = position + offset;
                if (!map.TryGetValue(neighborPos, out var neighbor))
                {
                    continue;
                }

                visible.Add(neighborPos);
                if (neighbor.IsWalkable() && distanceByPosition.TryAdd(neighborPos, distance + 1))
                {
                    queue.Enqueue(neighborPos);
                }
            }
        }

        return visible.Select(pos => map[pos]).ToList();
    }
}
