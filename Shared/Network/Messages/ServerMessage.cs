using Shared.Network;

namespace Shared.Network.Messages;

public class ServerMessage<T> where T : ISerializable<T>, IServerMessageable
{
    public T Content;

    public ServerMessage(byte[] bytes)
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

        writer.Write((byte) T.MessageType);
        requestObj.Encode(writer);

        byte[] arr = stream.ToArray();

        Console.WriteLine($"{arr.Length} bytes were sent to the client");

        return stream.ToArray();
    }
}