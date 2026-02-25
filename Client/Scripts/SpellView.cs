using System;
using System.Collections.Generic;
using System.Threading;
using Godot;
using Shared.Magic;
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
    Spell _spell;
    private Dictionary<string, Node2D> _containers;
    public override void _Ready()
    {
        GlobalLogger.Instance.SetLogFunction(GD.Print);	

        _containers = new(){
            ["slots"] = GetNode<Node2D>("Slots"),
            ["runes"] = GetNode<Node2D>("Runes"),
            ["links"] = GetNode<Node2D>("Links")
        };

        _spell = new Spell(5);
        _spell.AddRune(new Rune{ Type = RuneType.Source, Pos = Int2.Zero });
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

        for(int x = -_spell.Size + 1; x < _spell.Size ; x++) {
            for(int y = -_spell.Size  + 1; y < _spell.Size ; y++) {
                if((x > 0 && y > 0) || (x < 0 && y < 0) || Math.Abs(x) + Math.Abs(y) < _spell.Size) {
                    var hexPos = new Int2(x, y);

                    var slotInstance = SpellSlotScene.Instantiate<RuneSlotView>();
                    _containers["slots"].AddChild(slotInstance);
                    slotInstance.Initialize(hexPos);
                    slotInstance.Position = Coords.ToHexCenter(hexPos);
                    slotInstance.Name = $"Slot {hexPos}";
                    slotInstance.Clicked += OnSlotClicked;

                    if(rng.NextDouble() > 0.5)
                    {
                        _spell.AddRune(new Rune{ Type = RuneType.Conductor, Pos = hexPos, ActivationThreshold = 1000 });
                    }
                }
            }
        }
        GD.Print(_spell.LinkCache.Count);

        RenderContent();
    }

    public void RenderContent()
    {
        foreach(var child in _containers["links"].GetChildren())
        {
            child.QueueFree();
        }

        foreach(var child in _containers["runes"].GetChildren())
        {
            child.QueueFree();
        }

        foreach(var link in _spell.LinkCache)
        {
            var linkInstance = RuneLinkScene.Instantiate<RuneLinkView>();
            _containers["links"].AddChild(linkInstance);
            linkInstance.Initialize(link);
        }
        foreach(var rune in _spell.RuneCache)
        {
            var runeInstance = RuneScene.Instantiate<RuneView>();
            _containers["runes"].AddChild(runeInstance);
            runeInstance.Initialize(rune);
        }
    }

    double accumulator = 0;
    double tickDuration = 0.25;
    public override void _Process(double delta)
    {
        accumulator += delta;
        while(accumulator > tickDuration)
        {
            accumulator -= tickDuration;
            _spell.Update();
        }
    }

    public void OnSlotClicked(int x, int y, int mode)
    {
        switch(mode)
        {
            case 0:
                if(_spell.AddRune(new Rune { Type = RuneType.Conductor, Pos = new Int2(x, y), ActivationThreshold = 1000 }))
                {
                    RenderContent();
                }
                break;
            case 1:
                if(_spell.RemoveRune(new Int2(x, y)))
                {
                    RenderContent();
                }
                break;
            case 2:
                break;
        }
    }
}