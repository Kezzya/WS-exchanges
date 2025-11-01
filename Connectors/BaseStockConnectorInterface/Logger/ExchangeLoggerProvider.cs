using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BaseStockConnectorInterface.Logger
{
    public class ExchangeLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public ExchangeLoggerProvider(PrefixedLogger logger)
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

}
