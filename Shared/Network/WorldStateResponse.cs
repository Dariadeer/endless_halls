using Shared.Data;
using Shared.Data.Commands;
using Shared.Network.Messages;

namespace Shared.Network;

public class WorldStateResponse : ISerializable<WorldStateResponse>, IServerMessageable
{
    public int CurrentTick;
    public int SnapshotTick;
    public World World;
    public CommandList Commands;

    public static ServerMessageType MessageType => ServerMessageType.WorldState;

    public WorldStateResponse(int currentTick, int snapshotTick, World world, CommandList commands)
    {
        CurrentTick = currentTick;
        SnapshotTick = snapshotTick;
        World = world;
        Commands = commands;
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(CurrentTick);
        writer.Write(SnapshotTick);
        World.Encode(writer);
        Commands.Encode(writer);
    }

    public static WorldStateResponse Decode(BinaryReader reader)
    {
        return new WorldStateResponse(
            reader.ReadInt32(),
            reader.ReadInt32(),
            World.Decode(reader),
            CommandList.Decode(reader)
        );
    }
}