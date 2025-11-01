using Microsoft.Extensions.Logging;

namespace Digifinex.Net.Objects;

public class ExchangeLoggerProvider : ILoggerProvider
{
    private readonly ILogger _logger;

    public ExchangeLoggerProvider(ILogger logger)
    {
        _logger = logger;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _logger;
    }

    public void Dispose()
    {
        // If your custom logger has any resources to dispose, handle it here.
    }
}