using System.Net.Sockets;

namespace Server;

public sealed class Connection : IDisposable
{
    public TcpClient TcpClient { get; }
    public NetworkStream Stream { get; }

    public Connection(TcpClient client)
    {
        TcpClient = client;
        Stream = client.GetStream();
    }

    public ValueTask SendAsync(byte[] payload)
    {
        // Artificial server delay
        // await Task.Delay(200);
        byte[] packet = new byte[4 + payload.Length];
        BitConverter.GetBytes(payload.Length).CopyTo(packet, 0);
        payload.CopyTo(packet, 4);

        // Console.WriteLine($"{packet.Length} bytes were sent to the client");

        return Stream.WriteAsync(packet);
    }

    public void Dispose()
        => TcpClient.Dispose();
}
