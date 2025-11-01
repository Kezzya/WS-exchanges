using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using FameEX.Net.Models;
using FameEX.Net.Objects.Models.Socket;
using FameEX.Net.Objects.Socket.Query;
using Microsoft.Extensions.Logging;

namespace FameEX.Net.Objects.Socket.Subscriptions
{
    internal class FameexAllTickerSubscription : FameexSubscription<List<Models.Socket.FameexTicker>>
    {
        public FameexAllTickerSubscription(ILogger logger, Action<DataEvent<List<Models.Socket.FameexTicker>>> handler)
            : base(logger, new List<string>(), "all_ticker", "all_ticker.update", handler, false)
        {
        }

        public override CryptoExchange.Net.Sockets.Query? GetSubQuery(SocketConnection connection)
        {
            return new FameexQuery<FameexSubscriptionResult>("all_ticker.subscribe", Array.Empty<object>(), Authenticated);
        }

        public override CryptoExchange.Net.Sockets.Query? GetUnsubQuery()
        {
            return new FameexQuery<FameexSubscriptionResult>("all_ticker.unsubscribe", Array.Empty<object>(), Authenticated);
        }
    }
}