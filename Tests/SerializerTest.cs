using Shared.Data;
using Shared.Logic;
using Shared.Data.Commands;
using Shared.MyMath;
using Shared.Network;
using Tests.Utils;

namespace Tests;

public class SerializerTest
{
    [Fact]
    public void TestMoveCommandSerialization()
    {
        var move = new MoveCommand(123, 321, 345, new Int2(412, 5434));
        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(
            stream
        );

        move.Encode(writer);
        stream.Position = 0;

        BinaryReader reader = new BinaryReader(
            stream
        );


        var move2 = MoveCommand.Decode(reader);
        Assert.Equal(move.To, move2.To);
    }

    [Fact]
    public void TestSummonCommandSerialization()
    {
        var summon = new AppearCommand(123, 321, new Entity(412)
        {
            TeamId = 0,
            Pos = new(22523, 12414),
            Movement = new Movement(
                124, 415, new(31241, 14212)
            )
        });
        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(
            stream
        );


        summon.Encode(writer);
        stream.Position = 0;

        BinaryReader reader = new BinaryReader(
            stream
        );

        var summon2 = AppearCommand.Decode(reader);
        Assert.Equal(summon.To, summon2.To);
    }

    [Fact]
    public void TestWorldSnapshotSerialization()
    {
        World world = WorldGenerator.Simple();
        var stream = new MemoryStream();
        BinaryWriter writer = new(stream);

        world.Encode(writer);
        stream.Position = 0;

        BinaryReader reader = new(stream);

        var world2 = World.Decode(reader);

        foreach (var entity in world.Entities.Values)
        {
            var entity2 = world2.Entities[entity.Id];

            Assert.Equal(entity.Pos, entity2.Pos);
            Assert.Equal(entity.TeamId, entity2.TeamId);
            Assert.Equal(entity.Movement, entity2.Movement);
        }

        foreach (var tile in world.Grid.Values)
        {
            var tile2 = world2.Grid[tile.Pos];
            Assert.Equal(tile.Pos, tile2.Pos);
        }
    }

    [Fact]
    public void TestPlayerSerialization()
    {
        var player = new Player(12, "Player 12");
        var stream = new MemoryStream();
        BinaryWriter writer = new(stream);

        player.Encode(writer);
        stream.Position = 0;

        BinaryReader reader = new(stream);

        var player2 = Player.Decode(reader);
        
        Assert.Equal(player.Id, player2.Id);
        Assert.Equal(player.Name, player2.Name);
    }
}
