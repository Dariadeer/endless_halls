using Godot;

namespace Client.Source.Utils.Interfaces;

public interface ISceneNode
{
    static abstract string ScenePath { get; }

    static T Create<T>() where T : Node, ISceneNode
    {
        return GD.Load<PackedScene>(T.ScenePath).Instantiate<T>();
    }
}
