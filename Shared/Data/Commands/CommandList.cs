using Shared.Network;

namespace Shared.Data.Commands;

public class CommandList : ISerializable<CommandList>
{
    private LinkedList<ICommand> _commands = [];

    public void Add(ICommand command)
    {
        for(var node = _commands.Last; node != null; node = node.Previous)
        {
            if(node.Value.Id < command.Id)
            {
                _commands.AddAfter(node, command);
                return;
            }
        }

        _commands.AddFirst(command);
    }

    public LinkedList<ICommand> GetAll()
    {
        return _commands;
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
        writer.Write(_commands.Count);
        
        foreach (var command in _commands)
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