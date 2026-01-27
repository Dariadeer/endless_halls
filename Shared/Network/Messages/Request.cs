using Shared.Network;

namespace Shared.Network.Messages;

public class Request<T> where T : ISerializable<T>, IRequestable
{
    public T Content;

    public Request(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var reader = new BinaryReader(stream);

        stream.Position = 1;
        Content = T.Decode(reader);
    }

    public static byte[] Generate(T requestObj)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write((byte) T.RequestType);
        requestObj.Encode(writer);

        return stream.ToArray();
    }
}