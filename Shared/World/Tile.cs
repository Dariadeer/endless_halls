using Shared.MyMath;
using Shared.Network;

namespace Shared.Data;
public class Tile : ISnapshot<Tile>, ISerializable<Tile>
{
    public readonly Int2 Pos;
    public byte ObstacleId = 0;

    public Tile(Int2 pos)
    {
        Pos = pos;
    }

    public Tile(Int2 pos, byte obstacleId)
    {
        Pos = pos;
        ObstacleId = obstacleId;
    }

    public bool IsWalkable()
    {
        return ObstacleId == 0;
    }

    public Tile Copy()
    {
        return new Tile(Pos, ObstacleId);
    }

    public void Encode(BinaryWriter writer)
    {
        Pos.Encode(writer);
        writer.Write(ObstacleId);
    }

    public static Tile Decode(BinaryReader reader)
    {
        return new Tile(Int2.Decode(reader), reader.ReadByte());
    }
}
