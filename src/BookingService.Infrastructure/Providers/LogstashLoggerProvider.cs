namespace BookingService.Infrastructure.Providers;
// LogstashLoggerProvider.cs
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

public class LogstashLoggerProvider : ILoggerProvider
{
    private readonly string _host;
    private readonly int _port;

    public LogstashLoggerProvider(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new LogstashLogger(_host, _port, categoryName);
    }

    public void Dispose() { }
}

public class LogstashLogger : ILogger
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _categoryName;

    public LogstashLogger(string host, int port, string categoryName)
    {
        _host = host;
        _port = port;
        _categoryName = categoryName;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, 
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var logEntry = new
        {
            timestamp = DateTime.UtcNow.ToString("o"),
            level = logLevel.ToString(),
            category = _categoryName,
            message = formatter(state, exception),
            exception = exception?.ToString()
        };

        var json = JsonSerializer.Serialize(logEntry);
        var data = Encoding.UTF8.GetBytes(json + "\n");

        try
        {
            using var client = new TcpClient(_host, _port);
            using var stream = client.GetStream();
            stream.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to send log to Logstash: {ex.Message}");
        }
    }
}
