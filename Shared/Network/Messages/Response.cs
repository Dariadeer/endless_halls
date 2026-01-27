using Shared.Network;

namespace Shared.Network.Messages;

public class Response<T> where T : ISerializable<T>, IRespondable
{
    public T Content;

    public Response(byte[] bytes)
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

        writer.Write((byte) T.ResponseType);
        requestObj.Encode(writer);

        return stream.ToArray();
    }
}