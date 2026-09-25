using Shared.MyMath;
using Shared.Utils;

namespace Shared.Magic;

public class Spell
{
    static readonly int2[] NeighborOffsets = [int2.Right, int2.Down, new int2(-1, -1), int2.Left, int2.Up, new int2(1, 1)];

    public readonly Dictionary<int2, Rune> Runes = [];
    public readonly Dictionary<(int2, int2), RuneLink> Links = [];

    private Dictionary<Rune, Mana> _initialTickMana = [];
    private List<int2> _isolationLayer = [];
    private SpellMode mode = SpellMode.Activation;

    // Spell area
    // Spell focus

    public int Size = 0;

    public Spell(int size)
    {
        Size = size;
    }

    public void Update()
    {
        if (mode == SpellMode.Activation)
        {
            Activate();
            mode = SpellMode.Propagation;
        }
        else
        {
            Propagate();
            mode = SpellMode.Activation;
        }

        // Activate();
        // Propagate();
    }

    public void Propagate()
    {
        foreach (var link in Links.Values)
        {
            link.TransferMana(_initialTickMana);
        }
    }

    public void Activate()
    {
        foreach (var rune in Runes.Values)
        {
            rune.Activate();
            _initialTickMana[rune] = rune.Mana;
        }
    }

    public bool AddRune(int2 pos, Rune rune)
    {
        if (Runes.ContainsKey(pos)) return false;
        Runes.Add(pos, rune);
        _initialTickMana[rune] = rune.Mana;
        GlobalLogger.Instance.Log($"Adding rune {rune.Type} at {pos}");
        UpdateNeighborLinks(pos, rune);
        return true;
    }

    public bool RemoveRune(int2 pos)
    {
        if (Runes.TryGetValue(pos, out var rune))
        {
            Runes.Remove(pos);
            for (int i = 0; i < NeighborOffsets.Length; i++)
            {
                int2 offset = NeighborOffsets[i];
                int2 neighborPos = pos + offset;
                if (Runes.TryGetValue(neighborPos, out var neighbor))
                {
                    if (TryRemoveLink(pos, neighborPos))
                    {
                        // UpdateNeighborLinks(neighborPos, Runes[neighborPos]);
                    }

                    if (rune.Type == RuneType.Isolator)
                    {
                        int2 offset2 = NeighborOffsets[(i + 1) % 6];
                        int2 neighborPos2 = pos + offset2;

                        if (Runes.TryGetValue(neighborPos2, out var neighbor2))
                        {
                            int2 possibleIsolatorPos = neighborPos + offset2;
                            if (!Runes.TryGetValue(possibleIsolatorPos, out var isolator) || isolator.Type != RuneType.Isolator)
                            {
                                var link = new RuneLink(neighbor, neighbor2, 0.2f);
                                Links[(neighborPos, neighborPos2)] = link;
                                GlobalLogger.Instance.Log($"Relinked {neighborPos} and {neighborPos2}");
                            }
                        }
                    }
                }
            }
            return true;
        }

        return false;
    }

    public void ResetRunes()
    {
        foreach (var rune in Runes.Values)
        {
            rune.Reset();
        }
    }

    public void UpdateNeighborLinks(int2 pos, Rune rune)
    {
        for (int i = 0; i < NeighborOffsets.Length; i++)
        {
            int2 offset = NeighborOffsets[i];
            int2 neighborPos = pos + offset;
            // Process linking if a neighbor rune exists
            if (Runes.TryGetValue(neighborPos, out var neighbor))
            {
                var link = new RuneLink(rune, neighbor, 0.2f);
                Links[(pos, neighborPos)] = link;
                GlobalLogger.Instance.Log($"Linked {pos} and {neighborPos}");
            }
        }

        if (rune.Type == RuneType.Isolator)
        {
            ApplyIsolation(pos);
        }

        TryApplyIsolationAround(pos);
    }

    public (int2, int2) GetRuneLinkPositions(Rune rune1, Rune rune2)
    {
        int2? pos1 = null, pos2 = null;
        foreach (var (pos, rune) in Runes)
        {
            if (rune == rune1)
            {
                pos1 = pos;
            }

            if (rune == rune2)
            {
                pos2 = pos;
            }
        }

        if (pos1 is int2 p1 && pos2 is int2 p2)
        {
            return (p1, p2);
        }
        else
        {
            throw new Exception("Connection between the runes does not exist.");
        }
    }

    public void ApplyIsolation(int2 pos)
    {
        GlobalLogger.Instance.Log($"Applying isolation around {pos}");
        for (int i = 0; i < 6; i++)
        {
            int2 pos1 = pos + NeighborOffsets[i];
            int2 pos2 = pos + NeighborOffsets[(i + 1) % 6];

            if (TryGetRuneLink(pos1, pos2, out var link))
            {
                TryRemoveLink(pos1, pos2);
                GlobalLogger.Instance.Log($"Breaking up {pos1} and {pos2}");
            }
        }
    }

    public void RemoveIsolation(int2 pos)
    {

        GlobalLogger.Instance.Log($"Removing isolation around {pos}");
        for (int i = 0; i < 6; i++)
        {
            int2 pos1 = pos + NeighborOffsets[i];
            int2 pos2 = pos + NeighborOffsets[(i + 1) % 6];

            if (TryGetRuneLink(pos1, pos2, out var link))
            {
                TryRemoveLink(pos1, pos2);
                GlobalLogger.Instance.Log($"Breaking up {pos1} and {pos2}");
            }
        }
    }

    public void TryApplyIsolationAround(int2 pos)
    {

        for (int i = 0; i < 6; i++)
        {
            int2 neighborPos = pos + NeighborOffsets[i];
            if (Runes.TryGetValue(neighborPos, out var rune) && rune.Type == RuneType.Isolator)
            {
                ApplyIsolation(neighborPos);
            }
        }
    }

    public bool TryGetRuneLink(int2 pos1, int2 pos2, out RuneLink? link)
    {
        return Links.TryGetValue((pos1, pos2), out link)
            || Links.TryGetValue((pos2, pos1), out link);
    }

    public bool TryRemoveLink(int2 pos1, int2 pos2)
    {
        return Links.Remove((pos1, pos2))
            || Links.Remove((pos2, pos1));
    }

    public bool AreNeighbors(int2 pos1, int2 pos2)
    {
        int2 diff = pos1 - pos2;
        return MathF.Max(MathF.Abs(diff.X), MathF.Abs(diff.Y)) == 1;
    }
}

public class NeighborRune
{
    public required Rune Rune;
    public int BlockedFor = 0;
}

public enum SpellMode
{
    Activation,
    Propagation
}
