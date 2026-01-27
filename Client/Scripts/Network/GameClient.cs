namespace Client.Scripts.Network;

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public sealed class GameClient : IDisposable
{
    private readonly TcpClient _client = new();
    private readonly CancellationTokenSource _cts = new();
    private Task _receiveTask;

    public event Action OnConnect;
    public event Action<byte[]> OnMessage;
    public event Action OnDisconnect;

    public async Task ConnectAsync(string host, int port)
    {
        await _client.ConnectAsync(host, port);
        OnConnect?.Invoke();

        _receiveTask = ReceiveLoopAsync(_cts.Token);
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            var stream = _client.GetStream();
            var buffer = new byte[4096];

            while (!ct.IsCancellationRequested)
            {
                int bytes = await stream.ReadAsync(buffer, ct);
                if (bytes == 0)
                    break;

                OnMessage?.Invoke(buffer[..bytes]);
            }
        }
        catch (OperationCanceledException)
        {
            // expected on disconnect
        }
        catch (IOException)
        {
            // socket closed
        }
        finally
        {
            OnDisconnect?.Invoke();
        }
    }

    public async Task DisconnectAsync()
    {
        _cts.Cancel();

        try
        {
            _client.Close(); // unblocks ReadAsync
        }
        catch { }

        if (_receiveTask != null)
        {
            try
            {
                await _receiveTask;
            }
            catch { }
        }
    }

    public ValueTask SendAsync(byte[] data)
        => _client.GetStream().WriteAsync(data);

    public void Dispose()
        => _client.Dispose();
}
