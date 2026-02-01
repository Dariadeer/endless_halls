using System;
using Godot;
using Shared.Utils;

namespace Client.Scripts.Utils;

public class GDLogger : ILogger
{
    public void Log(Object obj)
    {
        GD.Print(obj);
    }
}