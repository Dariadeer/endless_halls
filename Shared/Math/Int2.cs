using Shared.Network;

namespace Shared.MyMath;

public readonly struct Int2 : IEquatable<Int2>, ISerializable<Int2>
{
    public readonly int X;
    public readonly int Y;

    public Int2(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static readonly Int2 Zero = new(0, 0);
    public static readonly Int2 Up = new(0, -1);
    public static readonly Int2 Down = new(0, 1);
    public static readonly Int2 Left = new(-1, 0);
    public static readonly Int2 Right = new(1, 0);

    public static Int2 operator +(Int2 a, Int2 b)
    {
        return new(a.X + b.X, a.Y + b.Y);
    }

    public static Int2 operator -(Int2 a, Int2 b)
    {
        return new(a.X - b.X, a.Y - b.Y);
    }

    public static Int2 operator *(int n, Int2 v)
    {
        return new(n * v.X, n * v.Y);
    }

    public bool Equals(Int2 other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is Int2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public static bool operator ==(Int2 left, Int2 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Int2 left, Int2 right)
    {
        return !(left == right);
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(X);
        writer.Write(Y);
    }

    public static Int2 Decode(BinaryReader reader)
    {
        return new Int2(reader.ReadInt32(), reader.ReadInt32());
    }
}