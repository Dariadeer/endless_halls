using Shared.MyMath;

namespace Shared.Magic;

public class Rune
{
    public required Int2 Pos;
    public int Mana = 0;
    public int ActivationThreshold = 0;
    public bool Activated = false;
    public RuneType Type = RuneType.Conductor;
    public RuneLink[] Links = new RuneLink[6];
    public int OpenLinkCount = 0;
    public int ManaToShare = 0;

    public int AddLink(int side, RuneLink link)
    {
        Links[side] = link;
        return ++OpenLinkCount;
    }

    public int RemoveLink(int side)
    {
        Links[side] = null;
        return --OpenLinkCount;
    }
}

public enum RuneType : byte
{
    Source = 0,
    Conductor = 1
}