using Shared.Network;

namespace Shared.Network.Messages;

public class ClientMessage<T> where T : ISerializable<T>, IClientMessageable
{
    public T Content;

    public ClientMessage(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var reader = new BinaryReader(stream);

        stream.Position = 1;
        Content = T.Decode(reader);
    }

    public static byte[] Generate(T objToSend)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write((byte) T.MessageType);
        objToSend.Encode(writer);

        return stream.ToArray();
    }
}