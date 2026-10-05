using Shared.MyMath;

namespace Shared.Magic.Manifestation;

public sealed class Spell
{
    public Dictionary<int2, SpellEffect> Effects;

    public Spell()
    {
        Effects = [];
    }
}
