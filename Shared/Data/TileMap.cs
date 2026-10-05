using Shared.MyMath;
using Shared.Network;

namespace Shared.Data;

public class TileMap : Dictionary<int2, Tile>, ISnapshot<TileMap>, ISerializable<TileMap>
{
    public static int VisionRange = 5;

    private static readonly int2[] VisionDirections =
    [
        int2.Right,
        int2.Down,
        new int2(-1, -1),
        int2.Left,
        int2.Up,
        new int2(1, 1),
    ];

    public List<Tile> GetVisionArea(int2 origin)
    {
        var visible = new HashSet<int2>();

        // Cast the six rays from the origin. Include a wall, then stop that ray.
        foreach (var direction in VisionDirections)
        {
            var pos = origin;
            for (int distance = 0; distance <= VisionRange; distance++)
            {
                if (!TryGetValue(pos, out var tile))
                {
                    break;
                }

                visible.Add(pos);
                if (!tile.IsWalkable())
                {
                    break;
                }

                pos += direction;
            }
        }

        var candidates = new HashSet<int2>();
        AddNeighborsToCandidates(visible, candidates);

        // Expand the visible area in passes requiring progressively more
        // already-visible, walkable neighbors. Collect each pass before adding
        // it so tiles discovered in a pass do not chain-expand that same pass.
        for (int requiredNeighbors = 1; requiredNeighbors <= VisionDirections.Length; requiredNeighbors++)
        {
            var newlyVisible = new List<int2>();

            foreach (var candidatePos in candidates)
            {
                if (visible.Contains(candidatePos))
                {
                    continue;
                }

                int visibleWalkableNeighbors = 0;
                foreach (var direction in VisionDirections)
                {
                    var neighborPos = candidatePos + direction;
                    if (visible.Contains(neighborPos)
                        && TryGetValue(neighborPos, out var neighbor)
                        && neighbor.IsWalkable())
                    {
                        visibleWalkableNeighbors++;
                    }
                }

                if (visibleWalkableNeighbors >= requiredNeighbors)
                {
                    newlyVisible.Add(candidatePos);
                }
            }

            visible.UnionWith(newlyVisible);
            AddNeighborsToCandidates(newlyVisible, candidates);
        }

        return visible.Select(pos => this[pos]).ToList();
    }

    private void AddNeighborsToCandidates(IEnumerable<int2> positions, HashSet<int2> candidates)
    {
        foreach (var pos in positions)
        {
            foreach (var direction in VisionDirections)
            {
                var neighborPos = pos + direction;
                if (ContainsKey(neighborPos))
                {
                    candidates.Add(neighborPos);
                }
            }
        }
    }

    public void Generate(int radius)
    {
        Random rng = new();
        for (int x = -radius + 1; x < radius; x++)
        {
            for (int y = -radius + 1; y < radius; y++)
            {
                if ((x > 0 && y > 0) || (x < 0 && y < 0) || MathF.Abs(x) + MathF.Abs(y) < radius)
                {
                    var pos = new int2(x, y);
                    AddTile(
                        new Tile(pos, (byte)(pos.Distance(int2.Zero) > 2 && rng.NextDouble() > 0.67 ? 1 : 0))
                    );
                }
            }
        }
    }

    public void AddTile(Tile tile)
    {
        Add(tile.Pos, tile);
    }

    public Tile? GetOrNull(int2 pos)
    {
        return TryGetValue(pos, out var tile)
            ? tile
            : null;
    }

    public TileMap Copy()
    {
        var clone = new TileMap();

        foreach (var tile in Values)
        {
            clone[tile.Pos] = tile.Copy();
        }

        return clone;
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(Count);

        foreach (var tile in Values)
        {
            tile.Encode(writer);
        }
    }

    public static TileMap Decode(BinaryReader reader)
    {
        int count = reader.ReadInt32();

        var map = new TileMap();

        for (int i = 0; i < count; i++)
        {
            map.AddTile(Tile.Decode(reader));
        }

        return map;
    }
}
