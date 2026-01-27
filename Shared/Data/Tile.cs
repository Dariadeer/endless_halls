using Shared.Math;
using Shared.Network;

namespace Shared.Data;
public class Tile : ISnapshot<Tile>, ISerializable<Tile>
{
    public readonly Int2 Pos;

    public Tile(Int2 pos)
    {
        Pos = pos;
    }

    public Tile Copy()
    {
        return new Tile(Pos);
    }

    public void Encode(BinaryWriter writer)
    {
        Pos.Encode(writer);
    }

    public static Tile Decode(BinaryReader reader)
    {
        return new Tile(Int2.Decode(reader));
    }
}
