using Shared.Data.Commands;
using Shared.Network;
using Shared.Network.Messages;

namespace Shared.Data;

public class HaltCommand : ICommand, ISerializable<HaltCommand>, IClientMessageable, IServerMessageable
{
    public static ServerMessageType MessageType => ServerMessageType.Halt;

    static ClientMessageType IClientMessageable.MessageType => ClientMessageType.Halt;

    public int Id { get; }
    public int Tick { get; }
    public readonly int EntityId;

    public HaltCommand(int id, int tick, int entityId) 
    {
        EntityId = entityId;
        Tick = tick;
        Id = id;
    }

    public static HaltCommand Decode(BinaryReader reader)
    {
        return new(
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32()
        );
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(Id);
        writer.Write(Tick);
        writer.Write(EntityId);
    }
}
