using Shared.Magic.Elements;

namespace Shared.Magic.Runes;

public class SourceRune : Rune
{
    private Mana _production;

    public SourceRune(int capacity, int production)
        : base(capacity)
    {
        _production = Mana.From(
            new()
            {
                { ElementType.Pure, production }
            });
        Type = RuneType.Source;
    }

    public override void Activate()
    {
        ChangeMana(_production);
    }
}
