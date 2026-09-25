using Godot;
using Client.Scripts.Utils;
using Client.Scripts.Utils.Abstracts;

namespace Client.Scripts;

public partial class Main : Node2D
{
    public Menu _menu;
    private WorldView _currentWorldView;

    public Main()
    {
        _menu = SceneFactory.Create<Menu>();
        AddChild(_menu);
    }

    public override void _Ready()
    {
        _menu.LocalWorldRequested += LoadLocalWorld;
        _menu.RemoteWorldRequested += LoadRemoteWorld;
    }

    public void LoadLocalWorld()
    {
        RemoveChild(_menu);
        _currentWorldView = SceneFactory.Create<LocalWorldView>();
        _currentWorldView.Initialize([]);
        AddChild(_currentWorldView);
    }

    public void LoadRemoteWorld(string address)
    {
        RemoveChild(_menu);
        _currentWorldView = SceneFactory.Create<RemoteWorldView>();
        _currentWorldView.Initialize([address]);
        AddChild(_currentWorldView);
    }

    public override async void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed)
        {
            return;
        }

        switch (keyEvent.Keycode)
        {
            case Key.Escape:
                if (_currentWorldView is null)
                {
                    break;
                }

                await _currentWorldView.Terminate();
                _currentWorldView.QueueFree();
                AddChild(_menu);
                break;
        }
    }
}
