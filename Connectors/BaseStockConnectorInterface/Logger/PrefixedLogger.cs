using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BaseStockConnectorInterface.Logger
{
    public class PrefixedLogger : ILogger
    {
        ILogger _externalLogger;
        string _logPrefix;
        public PrefixedLogger(ILogger externalLogger, string logPrefix)
        {
            _externalLogger = externalLogger;
            _logPrefix = logPrefix;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel) => _externalLogger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var prefixedMessage = $"{_logPrefix,-10} | {formatter(state, exception)}";

            //var message = formatter.Invoke(state, exception);
            //message = $"{_logPrefix,-10} | {message}";
            //_externalLogger.Log(logLevel, eventId, state, exception, (state, exception) => {
            //    var message = formatter.Invoke(state, exception);
            //    message = $"{_logPrefix,-10} | {message}";
            //    return message;
            //});
            //_externalLogger.Log(logLevel, eventId, state, exception, (state, exception) => prefixedMessage);
            _externalLogger.Log(logLevel, eventId, exception, prefixedMessage);
            //Console.WriteLine($"{logLevel} {eventId} {exception?.Message} {formatter.Invoke(state, exception)}");
        }
    }
}
