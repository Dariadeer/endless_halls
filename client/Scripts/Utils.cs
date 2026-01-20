namespace Client.Scripts;

using System;
using Godot;
using Shared.Math;

public class Utils
{
    static float _sin30 = MathF.Sin(MathF.PI / 6);
    static float _sin60 = MathF.Sin(MathF.PI / 3);
    static float _tan60 = MathF.Tan(MathF.PI / 3);
    public static Vector2 ToHexCenter(Int2 hex, float radius) {
        return new Vector2(
            (1 + _sin30) * radius * hex.X,
            _sin60 * (2 * hex.Y - hex.X) * radius
        );
    }
}