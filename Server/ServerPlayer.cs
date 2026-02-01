using Shared.Network;

namespace Server;

public class ServerPlayer
{
    public Connection Connection;
    public Player Player;
    
    public ServerPlayer(Connection connection, Player player)
    {
        Connection = connection;
        Player = player;
    }
}