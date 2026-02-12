using System;
using Godot;
using Shared.Network;

namespace Client.Scripts.Network;

public class PingManager
{
    public static long PING_INTERVAL = 1000;
    private long[] _pings = new long[256];
	private byte nextPingId = 255;
    public long lastPingTime = 0;

    public ClientPing Make(int tick)
    {
        var time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        _pings[nextPingId] = time;
        // GD.Print($"{time} – ping {nextPingId} sent");
        return new ClientPing
        {
            Id = nextPingId++,
            Tick = tick
        };
    }

    public long GetDelay(ServerPing ping)
    {
        // GD.Print($"{DateTimeOffset.Now.ToUnixTimeMilliseconds()} – ping {ping.Id} received");
        return DateTimeOffset.Now.ToUnixTimeMilliseconds() - _pings[ping.Id];
    }
}