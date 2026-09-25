using Shared.Data;
using Shared.Data.Commands;
using Shared.Network;
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

        if (args.Length > 1 && args[0].Equals("--port"))
        {
            port = int.Parse(args[1]);
        }

        var grid = new TileMap();
        grid.Generate(10);

        var world = new World(grid, []);
        worldManager = new WorldManager(world);

        GameServer server = new(port);
        server.OnConnect += (client) =>
        {
            Console.WriteLine($"Client connected!");
        };
        server.OnDisconnect += OnDisconnect;
        server.OnMessage += OnMessage;
        var serverTask = server.StartAsync();
        Console.WriteLine($"Server started and listening on port {port}");

        var wmTask = worldManager.StartLoop();
        Console.WriteLine("A new world is now being processed!");

        await serverTask;
    }

    static async void OnMessage(Connection connection, byte[] bytes)
    {
        Debug.Assert(worldManager != null);

        var requestType = (ClientMessageType)bytes[0];

        switch (requestType)
        {
            case ClientMessageType.Join:
                var loginReq = new ClientMessage<JoinRequest>(bytes);

                int playerId = nextId++;
                var player = new Player(playerId, loginReq.Content.Name);
                await worldManager.AddPlayer(connection, player);
                await connection.SendAsync(ServerMessage<Player>.Generate(player));
                Console.WriteLine($"Welcome, player {player.Name} ({player.Id})");
                break;
            case ClientMessageType.WorldData:
                _ = connection.SendAsync(ServerMessage<WorldStateResponse>.Generate(worldManager.GetWorldData()));
                Console.WriteLine("Sent world data!");
                break;
            case ClientMessageType.Move:
                var moveIntent = new ClientMessage<MoveCommand>(bytes).Content;
                worldManager?.ProcessMovement(connection, moveIntent);
                break;
            case ClientMessageType.Halt:
                var halt = new ClientMessage<HaltCommand>(bytes).Content;
                worldManager.ProcessHalt(connection, halt);
                break;
            case ClientMessageType.Ping:
                var ping = new ClientMessage<ClientPing>(bytes).Content;
                // Console.WriteLine($"{DateTimeOffset.Now.ToUnixTimeMilliseconds()} - ping {ping.Id} received");
                _ = connection.SendAsync(ServerMessage<ServerPing>.Generate(
                    new ServerPing
                    {
                        Id = ping.Id
                    }
                ));
                // Console.WriteLine($"{DateTimeOffset.Now.ToUnixTimeMilliseconds()} - ping {ping.Id} received");
                break;
        }
    }

    static async void OnDisconnect(Connection client)
    {
        worldManager?.RemovePlayer(client);
        Console.WriteLine("Client disconnected");
    }
}
