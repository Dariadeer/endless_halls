using Godot;
using Shared.Magic;
using Shared.MyMath;

namespace Client.Scripts;

public partial class RuneLinkView : Node2D
{
    private RuneLink _link;
    private Line2D _line;
    private int2 _pos1;
    private int2 _pos2;
    public void Initialize(int2 pos1, int2 pos2, RuneLink link)
    {
        _link = link;
        _line = GetNode<Line2D>("Line2D");
        _pos1 = pos1;
        _pos2 = pos2;
        Name = $"Link {pos1}-{pos2}";

        CallDeferred(nameof(Render));
    }

    public void Render()
    {
        _line.SetPointPosition(0, Coords.ToHexCenter(_pos1));
        _line.SetPointPosition(1, Coords.ToHexCenter(_pos2));
    }

    public override void _Process(double delta)
    {
        _line.DefaultColor = SpellView.PASSIVE_COLOR.Lerp(
            SpellView.ACTIVE_COLOR,
            Mathf.Abs(_link.ManaExchanged) / (Mathf.Min(_link.rune1.Capacity, _link.rune2.Capacity) * _link.Conductivity));
    }
}
