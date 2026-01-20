using Shared.Math;

namespace Shared.Logic.Commands;

public readonly struct MoveCommand : ICommand
{
    public int Id
    {
        get;
    }
    public readonly int EntityId;
    public readonly Int2 To;
}