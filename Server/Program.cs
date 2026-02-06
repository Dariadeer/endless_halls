using Shared.Data;
using Shared.Logic;
using Shared.Data.Commands;
using Shared.Math;
using Shared.Network;
using System.Net.Sockets;
using Shared.Network.Messages;
using System.Diagnostics;

namespace Server;

class Program
{
    static WorldManager? worldManager;
    static int nextId = 0;
    static async Task Main(string[] args)
    {
        int port = 3000;

        if(args.Length > 0)
        {
            port = int.Parse(args[0]);
        }

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

        GameServer server = new(3000);
        server.OnConnect += (client) =>
        {
            Console.WriteLine($"Client connected!");
        };
        server.OnDisconnect += OnDisconnect;
        server.OnMessage += OnMessage;
        var serverTask = server.StartAsync();
        Console.WriteLine("Server started and listening on port 3000");

        var wmTask = worldManager.StartLoop();
        Console.WriteLine("A new world is now being processed!");

        await serverTask;
    }

    static async void OnMessage(Connection client, byte[] bytes)
    {
        Debug.Assert(worldManager != null);

        var requestType = (ClientMessageType) bytes[0];

        switch (requestType)
        {
            case ClientMessageType.Join:
                var loginReq = new ClientMessage<JoinRequest>(bytes);

                int playerId = nextId++;
                var player = new Player(playerId, loginReq.Content.Name);
                worldManager.AddPlayer(client, player);
                _ = client.SendAsync(ServerMessage<Player>.Generate(player));
                break;
            case ClientMessageType.WorldData:
                _ = client.SendAsync(ServerMessage<WorldStateResponse>.Generate(worldManager.GetWorldData()));
                break;
            case ClientMessageType.Move:
                var moveIntent = new ClientMessage<MoveCommand>(bytes).Content;
                worldManager?.ProcessMovement(client, moveIntent);
                break;
            case ClientMessageType.Ping:
                var ping = new ClientMessage<ClientPing>(bytes).Content;
                worldManager.ProcessPing(client, ping);
                break;
        }
    }

    static async void OnDisconnect(Connection client)
    {
        worldManager?.RemovePlayer(client);
    }
}