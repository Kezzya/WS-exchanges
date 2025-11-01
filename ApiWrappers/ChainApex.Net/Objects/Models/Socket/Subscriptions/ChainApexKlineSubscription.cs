using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;

namespace ChainApex.Net.Objects.Models.Socket.Subscriptions
{
    internal class ChainApexKlineSubscription : ChainApexSubscription<ChainApexSocketKline>
    {
        public ChainApexKlineSubscription(ILogger logger, string symbol, string interval, Action<DataEvent<ChainApexSocketKline>> handler)
            : base(logger, symbol, $"kline_{interval}", handler)
        {
        }
    }
}

