using Shared.MyMath;

namespace Shared.Data;

public sealed class TileMapGenerator(int seed = 1)
{
    private readonly int _seed = seed;

    public Tile GenerateTile(int2 position, IReadOnlySet<int2>? protectedPositions = null)
    {
        if (protectedPositions?.Contains(position) == true)
        {
            return new Tile(position);
        }

        uint hash = unchecked((uint)_seed);
        hash ^= unchecked((uint)position.X) * 0x9E3779B9u;
        hash ^= unchecked((uint)position.Y) * 0x85EBCA6Bu;
        hash ^= hash >> 16;
        hash *= 0x7FEB352Du;
        hash ^= hash >> 15;
        hash *= 0x846CA68Bu;
        hash ^= hash >> 16;

        byte obstacleId = hash % 3 == 0 ? (byte)1 : (byte)0;
        return new Tile(position, obstacleId);
    }
}
