using Shared.Data.Commands;

namespace Tests;

public class DataStructuresTest
{
    [Fact]
    public async Task TestCommandList()
    {
        var cmdl = new CommandList();

        cmdl.Add(new MoveCommand(0, 0, 0, new Shared.MyMath.Int2(0, 1)));

        Assert.Single(cmdl.GetAll());
    }
}