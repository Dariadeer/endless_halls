using Shared.Magic.Elements;

namespace Shared.Magic;

public struct Mana
{
    public static readonly int ElementQuantity = 8;

    public float[] Quantities = new float[ElementQuantity];

    public Mana()
    {

    }

    public float Total
    {
        get
        {
            float total = 0;
            for (int i = 0; i < ElementQuantity; i++)
            {
                total += Quantities[i];
            }

            return total;
        }
    }

    public Mana ApplyOverflow(float capacity)
    {
        float ratio = capacity / Total;
        Mana normalized = new();
        for (int i = 0; i < ElementQuantity; i++)
        {
            normalized.Quantities[i] *= ratio;
        }
        return normalized;
    }

    public Mana Copy()
    {
        Mana copy = new();
        Quantities.CopyTo(copy.Quantities);
        return copy;
    }

    public override string ToString()
    {
        return $"M:{Total}:[{string.Join(", ", Quantities)}]";
    }

    public static Mana operator +(Mana mana1, Mana mana2)
    {
        Mana mana = new();
        for (int i = 0; i < ElementQuantity; i++)
        {
            mana.Quantities[i] = mana1.Quantities[i] + mana2.Quantities[i];
        }
        return mana;
    }

    public static Mana operator -(Mana mana)
    {
        Mana negated = new();
        for (int i = 0; i < ElementQuantity; i++)
        {
            negated.Quantities[i] = -mana.Quantities[i];
        }
        return negated;
    }

    public static Mana Difference(Mana mana1, Mana mana2, float conductivity)
    {
        Mana mana = new();
        for (int i = 0; i < ElementQuantity; i++)
        {
            mana.Quantities[i] = (mana1.Quantities[i] - mana2.Quantities[i]) * conductivity;
        }
        return mana;
    }

    public static Mana From(Dictionary<ElementType, float> values)
    {
        Mana mana = new();
        foreach (var entry in values)
        {
            mana.Quantities[(byte)entry.Key] = entry.Value;
        }
        return mana;
    }
}
