using Shared.Network;

namespace Shared.Data.Commands;

public class CommandList : LinkedList<ICommand>, ISerializable<CommandList>
{
    public void Add(ICommand command)
    {
        for(var node = Last; node != null; node = node.Previous)
        {
            if(node.Value.Id < command.Id)
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
        var commandList = new CommandList();

        for(int i = 0; i < count; i++)
        {
            var commandType = (CommandType) reader.ReadByte();

            ICommand command = commandType switch
            {
                CommandType.MoveCommand => MoveCommand.Decode(reader),
                CommandType.SummonCommand => SummonCommand.Decode(reader),
                _ => throw new ArgumentException("Unrecognized command")
            };

            commandList.Add(command);
        }

        return commandList;
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(Count);
        
        foreach (var command in this)
        {
            switch (command)
            {
                case MoveCommand mc:
                    writer.Write((byte) CommandType.MoveCommand);
                    mc.Encode(writer);
                    break;
                case SummonCommand sc:
                    writer.Write((byte) CommandType.SummonCommand);
                    sc.Encode(writer);
                    break;
                default:
                    throw new ArgumentException("Unrecognized command");
            }
        }
    }
}