namespace Shared.Network;

public interface ISerializable<T> where T : ISerializable<T>
{
    public void Encode(BinaryWriter writer);
    public static abstract T Decode(BinaryReader reader);
}