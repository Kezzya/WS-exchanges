using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CryptoExchange.Net.RateLimiting;
using BaseStockConnectorInterface.Helper;

namespace StockConnector
{
    public interface IStockHttpConnector : IAsyncDisposable
    {
        public event Action<RateLimitEvent> RateLimitTriggered;

        public virtual async Task ConnectAsync(IStockCredential stockCredential, ILogger clientLogger)
        {
            await ConnectAsync(stockCredential, clientLogger, Array.Empty<WebProxy>().ToList());
        }

        public virtual async Task ConnectAsync(IStockCredential stockCredential, ILogger clientLogger, List<WebProxy>? proxies)
        {
            ProxyRoundRobinHttpClientHandler? handler = null;

            if (proxies != null && proxies.Any())
                handler = new ProxyRoundRobinHttpClientHandler(proxies, clientLogger);

            await ConnectAsync(stockCredential, clientLogger, proxies, handler);
        }

        public void AddRateLimitLogging(string exchangeName, string accountId, string taskId, Func<string, Task> sendNotification)
        {
            RateLimitTriggered += async (evt) =>
            {
                var notification = new RateLimitExceededMessage
                {
                    Exchange = exchangeName,
                    Account = accountId,
                    TaskId = taskId,
                    Proxy = evt.RequestDefinition.Proxy?.ToString() ?? "",
                    RequestPath = evt.RequestDefinition?.Path,
                    Description = evt.LimitDescription,
                    Current = evt.Current,
                    RequestWeight = evt.RequestWeight,
                    RateLimitBehaviour = evt.Behaviour.ToString(),
                    Timestamp = DateTime.UtcNow,
                };

                await sendNotification(notification.ToString());
            };
        }

        public Task ConnectAsync(IStockCredential stockCredential, ILogger clientLogger, WebProxy proxy);
        public Task ConnectAsync(IStockCredential stockCredential, ILogger clientLogger, List<WebProxy>? proxies, HttpClientHandler? httpClientHandler);
        public Task DisconnectAsync();
    }
}
