using System;
using System.Collections.Generic;
using Godot;
using Shared.Magic;
using Shared.Magic.Elements;
using Shared.Magic.Runes;
using Shared.MyMath;
using Shared.Utils;

namespace Client.Scripts;

public partial class SpellView : Node2D
{
    public static Color ACTIVE_COLOR = new Color("#0000ff");
    public static Color PASSIVE_COLOR = new Color("#000000");
    [Export]
    public PackedScene SpellSlotScene;
    [Export]
    public PackedScene RuneScene;
    [Export]
    public PackedScene RuneLinkScene;

    private Dictionary<string, Node2D> _views;
    private RuneType _nextRuneType = RuneType.Conduit;

    Spell _spell;
    public override void _Ready()
    {
        GlobalLogger.Instance.SetLogFunction(GD.Print);

        _views = new()
        {
            ["slots"] = GetNode<Node2D>("Slots"),
            ["runes"] = GetNode<Node2D>("Runes"),
            ["links"] = GetNode<Node2D>("Links")
        };

        _spell = new Spell(5);
        _spell.AddRune(
            int2.Zero,
            new SourceRune(20, 10)
        );
        // _spell.AddRune(new Int2(0, 1), new Rune{ Type = RuneType.Conductor });
        // _spell.AddRune(new Int2(1, 1), new Rune{ Type = RuneType.Conductor });

        // Render background (empty slots)
        Render();
    }

    private void Render()
    {
        // foreach(var child in GetChildren())
        // {
        //     child.QueueFree();
        // }

        var rng = new Random();

        for (int x = -_spell.Size + 1; x < _spell.Size; x++)
        {
            for (int y = -_spell.Size + 1; y < _spell.Size; y++)
            {
                if ((x > 0 && y > 0) || (x < 0 && y < 0) || Math.Abs(x) + Math.Abs(y) < _spell.Size)
                {
                    var hexPos = new int2(x, y);

                    var slotInstance = SpellSlotScene.Instantiate<RuneSlotView>();
                    _views["slots"].AddChild(slotInstance);
                    slotInstance.Initialize(hexPos);
                    slotInstance.Position = Coords.ToHexCenter(hexPos);
                    slotInstance.Name = $"Slot {hexPos}";
                    slotInstance.Clicked += OnSlotClicked;

                    // Randomly generate runes
                    // if (rng.NextDouble() > 0.5)
                    // {
                    //     _spell.AddRune(new SpellRune(
                    //         new Rune(),
                    //         new Int2(x, y)
                    //     ));
                    //     GD.Print("Rune");
                    // }
                }
            }
        }
        GD.Print(_spell.Links.Count);

        RenderContent();
    }

    public void RenderContent()
    {
        foreach (var child in _views["links"].GetChildren())
        {
            child.QueueFree();
        }

        foreach (var child in _views["runes"].GetChildren())
        {
            child.QueueFree();
        }

        foreach (var ((pos1, pos2), link) in _spell.Links)
        {
            var linkInstance = RuneLinkScene.Instantiate<RuneLinkView>();
            _views["links"].AddChild(linkInstance);
            linkInstance.Initialize(pos1, pos2, link);
        }
        foreach (var (pos, rune) in _spell.Runes)
        {
            var runeInstance = RuneScene.Instantiate<RuneView>();
            _views["runes"].AddChild(runeInstance);
            runeInstance.Initialize(rune, pos);
        }
    }

    double accumulator = 0;
    double tickDuration = 0.25;
    public override void _Process(double delta)
    {
        accumulator += delta;
        while (accumulator > tickDuration)
        {
            accumulator -= tickDuration;
            _spell.Update();
        }
    }

    public void OnSlotClicked(int x, int y, int mode)
    {
        int2 pos = new(x, y);

        if (mode == 1)
        {
            if (_spell.RemoveRune(pos))
            {
                RenderContent();
            }
            return;
        }

        bool runeAdded = false;
        switch (_nextRuneType)
        {
            case RuneType.Conduit:
                runeAdded = _spell.AddRune(pos, new Rune(10));
                break;
            case RuneType.Drain:
                runeAdded = _spell.AddRune(pos, new DrainRune(10, 1));
                break;
            case RuneType.Source:
                runeAdded = _spell.AddRune(pos, new SourceRune(20, 10));
                break;
            case RuneType.Isolator:
                runeAdded = _spell.AddRune(pos, new IsolationRune(10));
                break;
            case RuneType.Elemental:
                runeAdded = _spell.AddRune(pos, new ElementalRune(10, 0.2f, ElementType.Fire));
                break;
        }

        if (runeAdded)
        {
            RenderContent();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (int.TryParse(keyEvent.AsTextKeycode(), out int num))
            {
                _nextRuneType = (RuneType)(byte)num;
            }
            switch (keyEvent.Keycode)
            {
                case Key.R:
                    _spell.ResetRunes();
                    break;
            }
        }
    }
}
