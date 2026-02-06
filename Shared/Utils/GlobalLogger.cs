namespace Shared.Utils;

public class GlobalLogger
{
    private static GlobalLogger? _instance;
    private static readonly object _lock = new object();
    
    public delegate void LogDelegate(params object[] args);
    
    private LogDelegate _logFunction = (args) => Console.WriteLine(string.Join(" ", args));
    
    private GlobalLogger() { }
    
    public static GlobalLogger Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new GlobalLogger();
                    }
                }
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// Sets a custom log function.
    /// </summary>
    public void SetLogFunction(LogDelegate logFunction)
    {
        _logFunction = logFunction ?? throw new ArgumentNullException(nameof(logFunction));
    }
    
    /// <summary>
    /// Logs a message using the configured log function.
    /// </summary>
    public void Log(object obj)
    {
        _logFunction(obj);
    }
}