using Shared.Magic.Elements;
using Shared.Utils;

namespace Shared.Magic.Runes;

public class ElementalRune : Rune
{
    private float _conversion;
    private ElementType _element;

    public ElementalRune(int capacity, float conversion, ElementType element)
        : base(capacity)
    {
        Type = RuneType.Elemental;
        _conversion = conversion;
        _element = element;
    }

    public override void Activate()
    {
        float pure = Mana.Quantities[(int)ElementType.Pure];

        if (pure == 0)
        {
            return;
        }

        float converted = MathF.Min(0.1f + pure * _conversion, pure);
        pure -= converted;
        Mana.Quantities[(int)ElementType.Pure] = pure;
        Mana.Quantities[(int)_element] += converted;

        GlobalLogger.Instance.Log($"Converted {converted} mana, total of {Mana}");
    }
}
