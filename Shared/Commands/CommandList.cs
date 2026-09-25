using Shared.Network;
using Shared.Utils;

namespace Shared.Data.Commands;

public class CommandList : LinkedList<ICommand>, ISerializable<CommandList>
{
    public void Add(ICommand command)
    {
        for (var node = Last; node != null; node = node.Previous)
        {
            if (node.Value.Id < command.Id)
            {
                AddAfter(node, command);
                return;
            }
        }

        AddFirst(command);
    }

    public static CommandList Decode(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        GlobalLogger.Instance.Log($"Cmd count: {count}");
        var commandList = new CommandList();

        for (int i = 0; i < count; i++)
        {
            var commandType = (CommandType)reader.ReadByte();

            ICommand command = commandType switch
            {
                CommandType.MoveCommand => MoveCommand.Decode(reader),
                CommandType.AppearCommand => AppearCommand.Decode(reader),
                CommandType.DisappearCommand => DisappearCommand.Decode(reader),
                CommandType.HaltCommand => HaltCommand.Decode(reader),
                _ => throw new ArgumentException($"Unrecognized command {commandType}")
            };

            commandList.Add(command);
            GlobalLogger.Instance.Log(command);
        }

        return commandList;
    }

    public void Encode(BinaryWriter writer)
    {
        GlobalLogger.Instance.Log($"Cmd count: {Count}");
        writer.Write(Count);

        foreach (var command in this)
        {
            switch (command)
            {
                case MoveCommand mc:
                    writer.Write((byte)CommandType.MoveCommand);
                    mc.Encode(writer);
                    break;
                case AppearCommand sc:
                    writer.Write((byte)CommandType.AppearCommand);
                    sc.Encode(writer);
                    break;
                case DisappearCommand dc:
                    writer.Write((byte)CommandType.DisappearCommand);
                    dc.Encode(writer);
                    break;
                case HaltCommand hc:
                    writer.Write((byte)CommandType.HaltCommand);
                    hc.Encode(writer);
                    break;
                default:
                    GlobalLogger.Instance.Log($"Unrecognized command in Command List: {command}");
                    break;
            }
        }
    }
}
