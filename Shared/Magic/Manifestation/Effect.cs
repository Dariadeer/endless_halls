using Shared.Data;

namespace Shared.Magic.Manifestation;

public delegate void EntityAction(Entity entity);

public sealed class SpellEffect
{
    public TimeSpan Duration { get; init; }

    public EntityAction? OnStart { get; init; }
    public EntityAction? OnTick { get; init; }
    public EntityAction? OnResolve { get; init; }
}
