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
        
        var world = new World(grid, entities);
        worldManager = new WorldManager(world);

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
        var requestType = (RequestType) bytes[0];

        switch (requestType)
        {
            case RequestType.Login:
                var loginReq = new Request<Login>(bytes);

                int playerId = nextId++;
                var player = new Player(playerId, loginReq.Content.Name);
                await client.SendAsync(Response<Player>.Generate(player));
                break;
            case RequestType.WorldData:
                await client.SendAsync(Response<World>.Generate(worldManager.GetWorldData()));
                break;
        }
    }
}