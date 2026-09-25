using Shared.Magic.Elements;

namespace Shared.Magic.Runes;

public class DrainRune : Rune
{
    private Mana _consumption;
    public DrainRune(float capacity, float consumption)
        : base(capacity)
    {
        _consumption = Mana.From(
            new()
            {
                { ElementType.Pure, consumption }
            }
        );
        Type = RuneType.Drain;
    }

    public override void Activate()
    {
        if (Mana.Total >= _consumption.Total)
        {
            ChangeMana(-_consumption);
        }
    }
}
