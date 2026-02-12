using Shared.Data.Commands;
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

    public static byte[] Generate(T objToSend)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write((byte) T.MessageType);
        objToSend.Encode(writer);

        byte[] arr = stream.ToArray();

        return stream.ToArray();
    }

    public static CommandType RecognizeCommandType(byte[] bytes)
    {
        return (CommandType) bytes[0];
    }
}