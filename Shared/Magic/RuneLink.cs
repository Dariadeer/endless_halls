namespace Shared.Magic;

public class RuneLink
{
    public Rune Rune1;
    public Rune Rune2;
    public int BlockedFor = 0;
    public bool ManaExchanged = false;

    public void ProcessManaExchange()
    {
        ManaExchanged = false;
        BlockedFor = Math.Max(BlockedFor - 1, 0);
        if(BlockedFor > 0) return;
        

        if(Rune1.ManaToShare > 0)
        {
            Rune2.Mana += Rune1.ManaToShare;
            Rune1.Mana -= Rune1.ManaToShare;
            ManaExchanged = true;

        }
        
        if(Rune2.ManaToShare > 0)
        {
            Rune1.Mana += Rune2.ManaToShare;
            Rune2.Mana -= Rune2.ManaToShare;
            ManaExchanged = true;
        }
        
        BlockedFor++;
    }

    public Rune GetOther(Rune rune)
    {
        if(Rune1 == rune) return Rune2;
        return Rune1;
    }
}