using Client.Scripts.Utils.Interfaces;
using Godot;

namespace Client.Scripts.Utils;

public static class SceneFactory
{
    public static T Create<T>() where T : Node, ISceneNode
    {
        return GD.Load<PackedScene>(T.ScenePath).Instantiate<T>();
    }
}

