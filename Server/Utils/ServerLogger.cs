using Shared.Utils;

namespace Server.Utils;

public class ServerLogger : ILogger
{
    public void Log(object obj)
    {
        Console.WriteLine(obj);
    }
}