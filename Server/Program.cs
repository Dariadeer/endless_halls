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

        if (args.Length > 0)
        {
            port = int.Parse(args[0]);
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
        Console.WriteLine("Server started and listening on port 3000");

        var wmTask = worldManager.StartLoop();
        Console.WriteLine("A new world is now being processed!");

        await serverTask;
    }

    static async void OnMessage(Connection client, byte[] bytes)
    {
        Debug.Assert(worldManager != null);

        var requestType = (ClientMessageType)bytes[0];

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
            case ClientMessageType.Halt:
                var halt = new ClientMessage<HaltCommand>(bytes).Content;
                worldManager.ProcessHalt(client, halt);
                break;
            case ClientMessageType.Ping:
                var ping = new ClientMessage<ClientPing>(bytes).Content;
                // Console.WriteLine($"{DateTimeOffset.Now.ToUnixTimeMilliseconds()} - ping {ping.Id} received");
                _ = client.SendAsync(ServerMessage<ServerPing>.Generate(
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
