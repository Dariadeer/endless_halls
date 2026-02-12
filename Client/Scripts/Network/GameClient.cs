namespace Client.Scripts.Network;

using System;
using System.Collections.Generic;
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
    private readonly List<byte> _recvBuffer = [];

    public event Action OnConnect;
    public event Action<byte[]> OnMessage;
    public event Action OnDisconnect;

    public async Task ConnectAsync(string host, int port)
    {
        _client.NoDelay = true;
        await _client.ConnectAsync(host, port);
        OnConnect?.Invoke();

        _receiveTask = Task.Run(() => ReceiveLoopAsync(_cts.Token));
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            var stream = _client.GetStream();
            var buffer = new byte[4096];

            while (!ct.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer, ct);
                if (bytesRead == 0)
                    break;

                // Append received bytes
                _recvBuffer.AddRange(buffer.AsSpan(0, bytesRead).ToArray());

                // Try to extract as many complete messages as possible
                while (TryReadMessage(out var message))
                {
                    OnMessage?.Invoke(message);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (IOException)
        {
        }
        finally
        {
            OnDisconnect?.Invoke();
        }
    }

    private bool TryReadMessage(out byte[] message)
    {
        message = null;

        // Need at least 4 bytes for length
        if (_recvBuffer.Count < 4)
            return false;

        int length = BitConverter.ToInt32(_recvBuffer.ToArray(), 0);

        // Optional sanity check
        if (length <= 0 || length > 10_000_000)
            throw new InvalidDataException("Invalid message length");

        // Wait until full payload is available
        if (_recvBuffer.Count < 4 + length)
            return false;

        message = _recvBuffer.GetRange(4, length).ToArray();
        _recvBuffer.RemoveRange(0, 4 + length);

        return true;
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
