using Godot;
using Shared.Magic;

namespace Client.Scripts;

public partial class RuneLinkView : Node2D
{
    private RuneLink _link;
    private Line2D _line;
    public void Initialize(RuneLink link)
    {
        _link = link;
        _line = GetNode<Line2D>("Line2D");
        Name = $"Link {_link.Rune1.Pos}-{_link.Rune2.Pos}";

        CallDeferred(nameof(Render));
    }

    public void Render()
    {
        _line.SetPointPosition(0, Coords.ToHexCenter(_link.Rune1.Pos));
        _line.SetPointPosition(1, Coords.ToHexCenter(_link.Rune2.Pos));
    }

    public override void _Process(double delta)
    {
        if(_link.ManaExchanged)
        {
            _line.DefaultColor = SpellView.ACTIVE_COLOR;
        } else
        {
            _line.DefaultColor = SpellView.PASSIVE_COLOR;
        }
    }
}