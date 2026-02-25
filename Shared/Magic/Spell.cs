using Shared.MyMath;
using Shared.Utils;

namespace Shared.Magic;

public class Spell
{
    public readonly Dictionary<Int2, Rune> Runes = [];
    static readonly Int2[] NeighborOffsets = [Int2.Right, Int2.Down, new Int2(-1, -1), Int2.Left, Int2.Up, new Int2(1, 1)];
    public readonly LinkedList<Rune> RuneCache = [];
    public readonly LinkedList<RuneLink> LinkCache = [];

    public int Size = 0;

    public Spell(int size)
    {
        Size = size;
    }

    public void Propagate()
    {
        foreach(var link in LinkCache)
        {
            link.ProcessManaExchange();
        }
    }

    public void Activate()
    {
        foreach(var rune in RuneCache)
        {
            switch (rune.Type)
            {
                case RuneType.Source:
                    rune.Mana += 60;
                    break;
            }

            if(rune.Mana >= rune.ActivationThreshold && !rune.Activated)
            {
                rune.Mana -= rune.ActivationThreshold;
                rune.Activated = true;
            }
            if(rune.Activated)
            {
                rune.ManaToShare = rune.Mana / Math.Max(1, rune.OpenLinkCount);
            }

            
        }
    }

    public void Update()
    {
        Activate();
        Propagate();
    }

    public bool AddRune(Rune rune)
    {
        if(Runes.ContainsKey(rune.Pos)) return false;
        Runes.Add(rune.Pos, rune);
        CreateNeighborLinks(rune);
        UpdateCache();
        return true;
    }

    public bool RemoveRune(Int2 pos)
    {
        if(Runes.TryGetValue(pos, out var rune))
        {
            Runes.Remove(pos);
            // Sever neighbor links
            for(int i = 0; i < 6; i++)
            {
                if(rune.Links[i] != null) 
                {
                    rune.Links[i].GetOther(rune).RemoveLink((i + 3) % 6);
                }
            }
            UpdateCache();
            return true;
        }
        return false;
    }

    public void UpdateCache()
    {
        RuneCache.Clear();
        LinkCache.Clear();

        Rune? rune;
        RuneLink link;
        for(int ring = 0; ring < Size; ring++)
        {
            foreach (var pos in GetRing(ring))
            {
                if(Runes.TryGetValue(pos, out rune))
                {
                    RuneCache.AddLast(rune);
                    
                    for(int i = 0; i < 6; i++)
                    {
                        link = rune.Links[i];
                        if(link == null || LinkCache.Contains(link)) continue;
                        LinkCache.AddLast(link);
                    }
                }
            }
        }
    }

    public void CreateNeighborLinks(Rune rune)
    {
        var pos = rune.Pos;
        Rune? neighbor;
        RuneLink link;
        Int2 neighborPos;
        for(int i = 0; i < 6; i++)
        {
            neighborPos = pos + NeighborOffsets[i];
            // GlobalLogger.Instance.Log($"Linking {rune.Pos} and {neighborPos}");
            if(Runes.TryGetValue(neighborPos, out neighbor))
            {
                link = new RuneLink
                {
                    Rune1 = rune,
                    Rune2 = neighbor
                };
                rune.AddLink(i, link);
                neighbor.AddLink((i + 3) % 6, link);

                GlobalLogger.Instance.Log($"Linked {rune.Pos} and {neighbor.Pos}");
            }
        }
    }

    public IEnumerable<Int2> GetRing(int radius)
    {
        if(radius == 0)
        {
            yield return Int2.Zero;
            yield break;
        }
        var current = new Int2(0, radius);
        for(int side = 0; side < 6; side++)
        {
            for(int step = 0; step < radius; step++)
            {
                yield return current;
                current += NeighborOffsets[side];
            }
        }
    }
}

public class NeighborRune
{
    public required Rune Rune;
    public int BlockedFor = 0;
}