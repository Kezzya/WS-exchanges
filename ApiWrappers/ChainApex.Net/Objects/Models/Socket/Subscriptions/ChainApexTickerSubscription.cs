using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;

namespace ChainApex.Net.Objects.Models.Socket.Subscriptions
{
    internal class ChainApexTickerSubscription : ChainApexSubscription<ChainApexSocketTicker>
    {
        public ChainApexTickerSubscription(ILogger logger, string symbol, Action<DataEvent<ChainApexSocketTicker>> handler)
            : base(logger, symbol, "ticker", handler)
        {
        }
    }
}

