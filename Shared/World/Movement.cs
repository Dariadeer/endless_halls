using Shared.MyMath;
using Shared.Network;

namespace Shared.Data;

public class Movement : ISerializable<Movement>
{
    public static readonly Movement Idle = new();
    public readonly MovementState State = MovementState.Idle;
    public readonly int Start;
    public readonly int End;
    public readonly Int2 To;

    public Movement(int start, int end, Int2 to)
    {
        State = MovementState.Moving;
        Start = start;
        End = end;
        To = to;
    }

    public Movement() { }

    public void Encode(BinaryWriter writer)
    {
        writer.Write((byte) State);
        if(State == MovementState.Moving)
        {
            writer.Write(Start);
            writer.Write(End);
            writer.Write(To.X);
            writer.Write(To.Y);
        }
    }

    public static Movement Decode(BinaryReader reader)
    {
        var state = (MovementState) reader.ReadByte();
        if(state == MovementState.Idle)
        {
            return new Movement();
        } else
        {
            return new Movement(
                reader.ReadInt32(),
                reader.ReadInt32(),
                Int2.Decode(reader)
            );
        }
    }
}

public enum MovementState : byte
{
    Idle = 0,
    Moving = 1
}