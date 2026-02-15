using Shared.MyMath;

namespace Shared.Magic;

public class Spell : Dictionary<Int2, Rune>
{
    static Int2[] AdjacencyOffsets = [Int2.Right, Int2.Down, new Int2(-1, -1), Int2.Left, Int2.Up, new Int2(1, 1)];

    public int Size = 0;

    public Spell(int size)
    {
        Size = size;
    }

    public void Propagate()
    {
        Rune rune;
        for(int ring = 0; ring < Size; ring++)
        {
            foreach (var pos in GetRing(ring))
            {
                if(TryGetValue(pos, out rune))
                {
                    // Cycle through neighbors and equally transfer mana
                }
            }
        }
    }

    public void Activate()
    {
        
    }

    public void Update()
    {
        Propagate();
        Activate();
    }

    public void AddRune(Int2 pos, Rune rune)
    {
        Add(pos, rune);
        // Link neighbors together
    }

    public IEnumerable<Int2> GetRing(int radius)
    {
        if(radius == 0)
        {
            yield return Int2.Zero;
            yield break;
        }
        var current = new Int2(0, radius);

        for(int side = 0; side < 5; side++)
        {
            for(int step = 0; step < radius; step++)
            {
                yield return current;
                current += AdjacencyOffsets[side];
            }
        }
    }
}

public class SpellSlot
{
    public Rune Rune;
    public List<Tuple<bool, Rune>> Adjacent = [];
}