namespace Shared.Magic;

public class Rune
{
    public Mana Mana;
    public float Overflow;
    public float Capacity;
    public RuneType Type = RuneType.Conduit;

    public Rune(float capacity)
    {
        Capacity = capacity;
        Mana = new Mana();
    }

    public void ChangeMana(Mana mana)
    {

        float newAmount = Mana.Total + mana.Total;
        if (newAmount > Capacity)
        {
            Mana.ApplyOverflow(Capacity);
            Overflow += newAmount - Capacity;
        }
        else
        {
            Mana = Mana + mana;
        }
    }

    public virtual void Activate()
    {

    }

    public void Reset()
    {
        Mana = new Mana();
        Overflow = 0;
    }
}

public enum RuneType : byte
{
    Source = 0,
    Conduit = 1,
    Drain = 2,
    Isolator = 3,
    Elemental = 4
}
