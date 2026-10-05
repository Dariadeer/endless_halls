namespace Shared.Magic;

public class RuneLink
{
    public Rune rune1;
    public Rune rune2;
    public float Conductivity;

    public float ManaExchanged = 0;

    public RuneLink(Rune rune1, Rune rune2, float conductivity)
    {
        this.rune1 = rune1;
        this.rune2 = rune2;
        Conductivity = conductivity;
    }

    public void TransferMana(Dictionary<Rune, Mana> initialTickMana)
    {
        float conductivity = Conductivity;
        if (rune1.Type == RuneType.Conduit && rune2.Type == RuneType.Conduit)
        {
            conductivity *= 2;
        }

        Mana diff = Mana.Difference(initialTickMana[rune1], initialTickMana[rune2], Conductivity);

        rune1.ChangeMana(-diff);
        rune2.ChangeMana(diff);

        ManaExchanged = diff.Total;
    }

    public Rune GetOther(Rune rune)
    {
        if (rune1 == rune) return rune2;
        return rune1;
    }
}
