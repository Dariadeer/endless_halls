using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;

public class GameServer
{
    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cts = new();
    private readonly List<Task> _clientTasks = new();

    public event Action<ClientConnection>? OnConnect;
    public event Action<ClientConnection, byte[]>? OnMessage;
    public event Action<ClientConnection>? OnDisconnect;

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

                var connection = new ClientConnection(tcpClient);
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


    private async Task HandleClientAsync(ClientConnection client, CancellationToken ct)
    {
        try
        {
            var stream = client.Stream;
            var buffer = new byte[4096];

            while (!ct.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer, ct);
                if (bytesRead == 0)
                    break;

                OnMessage?.Invoke(client, buffer[..bytesRead]);
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
}