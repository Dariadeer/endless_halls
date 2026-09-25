using Shared.Network;

namespace Shared.MyMath;

public readonly struct int2 : IEquatable<int2>, ISerializable<int2>
{
    public readonly int X;
    public readonly int Y;

    public int2(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static readonly int2 Zero = new(0, 0);
    public static readonly int2 Up = new(0, 1);
    public static readonly int2 Down = new(0, -1);
    public static readonly int2 Left = new(-1, 0);
    public static readonly int2 Right = new(1, 0);

    public static int2 operator +(int2 a, int2 b)
    {
        return new(a.X + b.X, a.Y + b.Y);
    }

    public static int2 operator -(int2 a, int2 b)
    {
        return new(a.X - b.X, a.Y - b.Y);
    }

    public static int2 operator *(int n, int2 v)
    {
        return new(n * v.X, n * v.Y);
    }

    public bool Equals(int2 other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is int2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public static bool operator ==(int2 left, int2 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(int2 left, int2 right)
    {
        return !(left == right);
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(X);
        writer.Write(Y);
    }

    public static int2 Decode(BinaryReader reader)
    {
        return new int2(reader.ReadInt32(), reader.ReadInt32());
    }
}
