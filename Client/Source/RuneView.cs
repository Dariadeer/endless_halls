using Godot;
using Shared.Magic;
using Shared.MyMath;

namespace Client.Scripts;

public partial class RuneView : Node2D
{
    private Rune _rune;
    private Line2D _outline;
    public void Initialize(Rune rune, int2 pos)
    {
        _rune = rune;
        Position = Coords.ToHexCenter(pos);
        Name = $"Rune {pos}";
        Render();
    }

    public override void _Ready()
    {
        _outline = GetNode<Line2D>("Line2D");
    }

    public override void _Process(double delta)
    {
        if (_rune.Mana.Total > 0)
        {
            _outline.DefaultColor = SpellView.PASSIVE_COLOR.Lerp(SpellView.ACTIVE_COLOR, _rune.Mana.Total / _rune.Capacity);
        }
        else
        {
            _outline.DefaultColor = SpellView.PASSIVE_COLOR;
        }
    }

    public void Render()
    {
        var polygon = GetNode<Polygon2D>("Polygon2D");
        Color? color = null;
        switch (_rune.Type)
        {
            case RuneType.Source:
                color = new Color("#aaaacc");
                break;
            case RuneType.Drain:
                color = new Color("#ccaaaa");
                break;
            case RuneType.Isolator:
                color = new Color("#555555");
                break;
            case RuneType.Elemental:
                color = new Color("#ff5555");
                break;
        }

        if (color is Color selectedColor)
        {
            polygon.Color = selectedColor;
        }
    }
}
