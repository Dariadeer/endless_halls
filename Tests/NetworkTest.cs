using Client.Scripts.Network;
using Server;
using Shared.Data;
using Shared.Logic;
using Shared.Network;
using Tests.Utils;

namespace Tests;
public class NetworkTest
{
    [Fact]
    public async Task TestClientServerConnectionAsync()
    {
        GameServer gs = new(4000);
        GameClient gc = new();

        World world = WorldGenerator.Simple();
        World? world2 = null;

        var testCompleted = new TaskCompletionSource();

        gs.OnConnect += async (c) =>
        {
            using MemoryStream ms = new();
            using BinaryWriter bw = new(ms);

            world.Encode(bw);

            await c.SendAsync(ms.ToArray());
        };

        gc.OnMessage += async (msg) =>
        {
            using MemoryStream ms = new(msg);
            using BinaryReader br = new(ms);

            ms.Position = 0;

            world2 = World.Decode(br);

            testCompleted.SetResult();
        };

        var serverTask = gs.StartAsync();

        // --- connect client ---
        await gc.ConnectAsync("127.0.0.1", 4000);

        // --- wait for test logic ---
        await testCompleted.Task;

        Assert.NotNull(world2);

        Assert.Equal(world.Grid.Count(), world2.Grid.Count());
        Assert.Equal(world.Entities.Count(), world2.Entities.Count());

        // --- shutdown ---
        await gc.DisconnectAsync();
        await gs.StopAsync();
        await serverTask;
    }
}