using System.Threading.Tasks;
using Godot;

namespace Client.Scripts.Utils.Abstracts;

public abstract partial class WorldView : Node
{
    public abstract void Initialize(string[] args);
    public abstract Task Terminate();
}
