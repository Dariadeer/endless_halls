using System.Net.Sockets;

namespace Server;

public sealed class ClientConnection : IDisposable
{
    public TcpClient TcpClient { get; }
    public NetworkStream Stream => TcpClient.GetStream();

    public ClientConnection(TcpClient client)
    {
        TcpClient = client;
    }

    public ValueTask SendAsync(byte[] data)
        => Stream.WriteAsync(data);

    public void Dispose()
        => TcpClient.Dispose();
}