using System.Net;
using System.Net.Sockets;

namespace Server;

public class GameServer
{
    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cts = new();
    private readonly List<Task> _clientTasks = [];


    public event Action<Connection>? OnConnect;
    public event Action<Connection, byte[]>? OnMessage;
    public event Action<Connection>? OnDisconnect;

    public GameServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public async Task StartAsync()
    {
        _listener.Start();

        try
        {
            while (!_cts.IsCancellationRequested)
            {
                TcpClient tcpClient;

                try
                {
                    tcpClient = await _listener.AcceptTcpClientAsync(_cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                tcpClient.NoDelay = true;
                var connection = new Connection(tcpClient);
                OnConnect?.Invoke(connection);

                var task = HandleClientAsync(connection, _cts.Token);
                _clientTasks.Add(task);
            }
        }
        finally
        {
            _listener.Stop();
        }
    }

    public async Task StopAsync()
    {
        _cts.Cancel();

        // Stop accepting new clients immediately
        _listener.Stop();

        // Wait for all client loops to finish
        try
        {
            await Task.WhenAll(_clientTasks);
        }
        catch
        {
            // swallow: clients may throw on shutdown
        }
    }


    private async Task HandleClientAsync(Connection client, CancellationToken ct)
    {
        try
        {
            var stream = client.Stream;
            var buffer = new byte[1024];
            List<byte> recvBuffer = [];

            while (!ct.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer, ct);
                if (bytesRead == 0)
                    break;

                // Append received bytes
                recvBuffer.AddRange(buffer.AsSpan(0, bytesRead).ToArray());

                // Try to extract as many complete messages as possible
                while (TryReadMessage(recvBuffer, out var message))
                {
                    if (message == null)
                    {
                        throw new Exception("The received message cannot be null");
                    }
                    OnMessage?.Invoke(client, message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // expected during shutdown
        }
        catch (IOException)
        {
            // client disconnected abruptly
        }
        finally
        {
            OnDisconnect?.Invoke(client);
            client.Dispose();
        }
    }

    private bool TryReadMessage(List<byte> recvBuffer, out byte[]? message)
    {
        message = null;

        // Need at least 4 bytes for length
        if (recvBuffer.Count < 4)
            return false;

        int length = BitConverter.ToInt32(recvBuffer.ToArray(), 0);

        // Optional sanity check
        if (length <= 0 || length > 10_000_000)
            throw new InvalidDataException("Invalid message length");

        // Wait until full payload is available
        if (recvBuffer.Count < 4 + length)
            return false;

        message = recvBuffer.GetRange(4, length).ToArray();
        recvBuffer.RemoveRange(0, 4 + length);

        return true;
    }
}
