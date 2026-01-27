using Shared.Data;
using Shared.Data.Commands;
using Shared.Network.Messages;

namespace Shared.Network;

public class WorldState : ISerializable<WorldState>, IServerMessageable
{
    public int CurrentTick;
    public int SnapshotTick;
    public World World;
    public CommandList Commands;

    public static ServerMessageType MessageType => ServerMessageType.WorldState;

    public WorldState(int currentTick, int snapshotTick, World world, CommandList commands)
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

    public static WorldState Decode(BinaryReader reader)
    {
        return new WorldState(
            reader.ReadInt32(),
            reader.ReadInt32(),
            World.Decode(reader),
            CommandList.Decode(reader)
        );
    }
}