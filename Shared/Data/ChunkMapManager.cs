using Shared.MyMath;

namespace Shared.Data;

public sealed class ChunkMapManager
{
    public const int ChunkRadius = 2;
    public const int GenerationRadius = 2;

    private readonly World _world;
    private readonly TileMapGenerator _generator;
    private readonly HashSet<int2> _generatedChunks = [];

    public ChunkMapManager(World world, TileMapGenerator generator)
    {
        _world = world;
        _generator = generator;
    }

    public void Update()
    {
        var entities = _world.Entities.Values.ToArray();
        if (entities.Length == 0)
        {
            return;
        }

        var protectedPositions = new HashSet<int2>();
        foreach (var entity in entities)
        {
            protectedPositions.Add(entity.Pos);
            protectedPositions.Add(entity.VisionOrigin);
        }

        bool addedTiles = false;
        foreach (var entity in entities)
        {
            var centerChunk = TileToChunk(entity.Pos, ChunkRadius);
            for (int dx = -GenerationRadius; dx <= GenerationRadius; dx++)
            {
                for (int dy = -GenerationRadius; dy <= GenerationRadius; dy++)
                {
                    var offset = new int2(dx, dy);
                    if (offset.Distance(int2.Zero) > GenerationRadius)
                    {
                        continue;
                    }

                    var chunk = centerChunk + offset;
                    if (_generatedChunks.Add(chunk))
                    {
                        addedTiles |= GenerateChunk(chunk, protectedPositions);
                    }
                }
            }
        }

        if (addedTiles)
        {
            _world.RefreshEntityVisionAreas();
        }
    }

    public static int2 ChunkToTile(int2 chunk, int radius)
    {
        return new int2(
            (1 + radius) * chunk.X + radius * chunk.Y,
            -radius * chunk.X + (1 + 2 * radius) * chunk.Y);
    }

    public static int2 TileToChunk(int2 tile, int radius)
    {
        long determinant = 3L * radius * radius + 3L * radius + 1;
        double chunkX = ((2L * radius + 1) * tile.X - (long)radius * tile.Y)
            / (double)determinant;
        double chunkY = ((long)radius * tile.X + (long)(radius + 1) * tile.Y)
            / (double)determinant;

        int estimatedX = (int)Math.Round(chunkX);
        int estimatedY = (int)Math.Round(chunkY);

        // Inverting the center transform gives a fractional estimate for tiles
        // away from chunk centers. Check nearby chunks against the exact hex
        // footprint to resolve which side of a chunk edge contains the tile.
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                var candidate = new int2(estimatedX + dx, estimatedY + dy);
                var localPosition = tile - ChunkToTile(candidate, radius);
                if (localPosition.Distance(int2.Zero) <= radius)
                {
                    return candidate;
                }
            }
        }

        throw new InvalidOperationException($"Could not find a chunk for tile position {tile} at radius {radius}.");
    }

    private bool GenerateChunk(int2 chunk, IReadOnlySet<int2> protectedPositions)
    {
        var center = ChunkToTile(chunk, ChunkRadius);
        bool addedTiles = false;

        for (int x = -ChunkRadius; x <= ChunkRadius; x++)
        {
            for (int y = -ChunkRadius; y <= ChunkRadius; y++)
            {
                var localPosition = new int2(x, y);
                if (localPosition.Distance(int2.Zero) > ChunkRadius)
                {
                    continue;
                }

                var tilePosition = center + localPosition;
                if (_world.Grid.ContainsKey(tilePosition))
                {
                    continue;
                }

                _world.Grid.AddTile(_generator.GenerateTile(tilePosition, protectedPositions));
                addedTiles = true;
            }
        }

        return addedTiles;
    }
}
