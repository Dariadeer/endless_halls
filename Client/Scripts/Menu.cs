using Godot;
using System;

namespace Client.Scripts;

public partial class Menu : Node
{
    [Export]
    public Button LocalWorldButton, RemoteWorldButton;

    [Export]
    public LineEdit IpAddressInput;

    [Signal]
    public delegate void LocalWorldRequestedEventHandler();
    [Signal]
    public delegate void RemoteWorldRequestedEventHandler(string ipAddress);

    public override void _Ready()
    {
        LocalWorldButton.Pressed += () =>
        {
            GD.Print("Loading a local world...");
            EmitSignal(SignalName.LocalWorldRequested);
        };

        RemoteWorldButton.Pressed += () =>
        {
            GD.Print($"Loading a remote world from {IpAddressInput.Text}");
            EmitSignal(SignalName.RemoteWorldRequested, IpAddressInput.Text);
        };
    }
}
