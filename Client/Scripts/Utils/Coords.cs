namespace Client.Scripts;

using System;
using Client.Scripts.Config;
using Godot;
using Shared.Data;
using Shared.Math;

public class Coords
{
    static float _sin30 = MathF.Sin(MathF.PI / 6);
    static float _sin60 = MathF.Sin(MathF.PI / 3);
    static float _tan60 = MathF.Tan(MathF.PI / 3);
    public static Vector2 ToHexCenter(Int2 hex) {
        return new Vector2(
            (1 + _sin30) * Globals.TileRadius * hex.X,
            _sin60 * (hex.X - 2 * hex.Y) * Globals.TileRadius
        );
    }

    public static Int2 ToHexCoords(Vector2 pos, float radius)
    {
        float q = pos.X / radius / (1 + _sin30);
        float r = (pos.Y / radius / _sin60 - q) / -2;

        float x = q;
        float z = r;
        float y = -x - z;

        int rx = (int)MathF.Round(x);
        int ry = (int)MathF.Round(y);
        int rz = (int)MathF.Round(z);

        float dx = MathF.Abs(rx - x);
        float dy = MathF.Abs(ry - y);
        float dz = MathF.Abs(rz - z);

        if (dx > dy && dx > dz)
            rx = -ry - rz;
        else if (dy > dz)
            ry = -rx - rz;
        else
            rz = -rx - ry;

        return new Int2((int) Math.Round(q), (int) Math.Round(r));
    }
}