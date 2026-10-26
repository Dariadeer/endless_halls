using Shared.Data.Vision;
using Shared.MyMath;
using Shared.Network;

namespace Shared.Data;

public class TileMap : Dictionary<int2, Tile>, ISnapshot<TileMap>, ISerializable<TileMap>
{
    public event Action<Tile>? TileAdded;
    public IVisionProvider VisionProvider { get; set; }

    public TileMap(IVisionProvider? visionProvider = null)
    {
        VisionProvider = visionProvider ?? new BreadthFirstSearchVisionProvider();
    }

    public List<Tile> GetVisionArea(int2 origin)
    {
        return VisionProvider.GetVisionArea(this, origin);
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
        TileAdded?.Invoke(tile);
    }

    public Tile? GetOrNull(int2 pos)
    {
        return TryGetValue(pos, out var tile)
            ? tile
            : null;
    }

    public TileMap Copy()
    {
        var clone = new TileMap(VisionProvider);

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
