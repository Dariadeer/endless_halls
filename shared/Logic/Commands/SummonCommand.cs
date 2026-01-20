using Shared.Data;

namespace Shared.Logic.Commands;

public readonly struct SummonCommand : ICommand
{
    public int Id
    {
        get;
    }
    public readonly int EntityId;
    public readonly EntityData EntityData;
}