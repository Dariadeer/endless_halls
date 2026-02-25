using Godot;
using Shared.Magic;
using Shared.MyMath;

namespace Client.Scripts;

public partial class RuneView : Node2D
{
    private Rune _rune;
    private Line2D _outline;
    public void Initialize(Rune rune)
    {
        _rune = rune;
        Position = Coords.ToHexCenter(_rune.Pos);
        Name = $"Rune {_rune.Pos}";
        Render();
    }

    public override void _Ready()
    {
        _outline = GetNode<Line2D>("Line2D");
    }

    public override void _Process(double delta)
    {
        if(_rune.Mana > 0)
        {
            _outline.DefaultColor = SpellView.ACTIVE_COLOR;
        } else
        {
            _outline.DefaultColor = SpellView.PASSIVE_COLOR;
        }
    }

    public void Render()
    {
        if(_rune.Type == RuneType.Source)
        {
            GetNode<Polygon2D>("Polygon2D").Color = new Color("#aaaaaa");
        }
    }
}