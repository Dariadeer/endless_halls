using Godot;
using System;

namespace Client.Scripts;

public partial class Main : Node2D
{
    [Export]
    public Menu Menu;

    [Export]
    public PackedScene LocalWorldScene;

    [Export]
    public PackedScene RemoteWorldScene;

    public override void _Ready()
    {
        Menu.LocalWorldRequested += LoadLocalWorld;
        Menu.RemoteWorldRequested += LoadRemoteWorld;
    }

    public void LoadLocalWorld()
    {
        RemoveChild(Menu);
        var instance = LocalWorldScene.Instantiate<LocalWorldView>();
        instance.Initialize();
        AddChild(instance);
    }

    public void LoadRemoteWorld(string address)
    {
        RemoveChild(Menu);
        var instance = RemoteWorldScene.Instantiate<RemoteWorldView>();
        instance.Main = this;
        instance.Initialize(address);
        AddChild(instance);
    }

    public void GoToMenu(Node source)
    {
        source.QueueFree();
        AddChild(Menu);
    }
}
