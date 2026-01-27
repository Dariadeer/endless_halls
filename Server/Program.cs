using Shared.Data;
using Shared.Logic;
using Shared.Data.Commands;
using Shared.Math;
using Shared.Network;
using System.Net.Sockets;
using Shared.Network.Messages;

namespace Server;

class Program
{
    static WorldManager? worldManager;
    static int nextId = 0;
    static async Task Main(string[] args)
    {
        var grid = new TileMap();
        grid.Generate(5);
        var entities = new EntityMap();
        entities.AddEntity(
            new Entity(-1)
            {
                Pos = new Int2(3, 0),
                TeamId = -1,
                Movement = new Movement()
            }
        );
        entities.AddEntity(
            new Entity(-2)
            {
                Pos = new Int2(-3, 0),
                TeamId = -1,
                Movement = new Movement()
            }
        );

        var entity = entities[-1];
        
        var world = new World(grid, entities);
        worldManager = new WorldManager(world);

        for(int i = 0; i < 100; i++)
        {
            Int2 to;
            if(i % 2 == 0)
            {
                to = new(2, 0);
            } else
            {
                to = new(3, 0);
            }
            worldManager.CommandQueue.Enqueue(
                new MoveCommand(0, i * 50, entity.Id, to)
            );
        }

        GameServer server = new(3000);
        server.OnConnect += (client) =>
        {
            Console.WriteLine($"Client connected!");
        };
        server.OnMessage += OnMessage;
        var serverTask = server.StartAsync();
        Console.WriteLine("Server started and listening on port 3000");

        var wmTask = worldManager.StartLoop();
        Console.WriteLine("A new world is now being processed!");

        await serverTask;
    }

    static async void OnMessage(ClientConnection client, byte[] bytes)
    {
        var requestType = (ClientMessageType) bytes[0];

        switch (requestType)
        {
            case ClientMessageType.Login:
                var loginReq = new ClientMessage<Login>(bytes);

                int playerId = nextId++;
                var player = new Player(playerId, loginReq.Content.Name);
                await client.SendAsync(ServerMessage<Player>.Generate(player));
                break;
            case ClientMessageType.WorldData:
                await client.SendAsync(ServerMessage<WorldState>.Generate(worldManager.GetWorldData()));
                break;
        }
    }
}