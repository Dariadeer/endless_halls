using Shared.Data;
using Shared.MyMath;
using Shared.Network;
using Shared.Utils;

namespace Shared.Date;

public class MoveQueue : Queue<Int2>, ISerializable<MoveQueue>, ISnapshot<MoveQueue>
{
    public MoveQueue() { }
    public MoveQueue(IEnumerable<Int2> collection) : base(collection) { }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(Count);

        foreach (var pos in this)
        {
            pos.Encode(writer);
        }
    }
    public static MoveQueue Decode(BinaryReader reader)
    {
        var result = new List<Int2>();

        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            result.Add(Int2.Decode(reader));
        }

        return new MoveQueue(result);
    }

    public MoveQueue Copy()
    {
        return new MoveQueue(this);
    }


}
