using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Digifinex.Net.Objects.Models.Socket;
using Digifinex.Net.Objects.Socket.Query;
using Microsoft.Extensions.Logging;

namespace Digifinex.Net.Objects.Socket.Subscriptions;

internal class DigifinexAllTickerSubscription : DigifinexSubscription<List<DigifinexTicker>>
{
    public DigifinexAllTickerSubscription(ILogger logger, Action<DataEvent<List<DigifinexTicker>>> handler)
        : base(logger, new List<string>(), "all_ticker", "all_ticker.update", handler, false)
    {
    }

    public override CryptoExchange.Net.Sockets.Query? GetSubQuery(SocketConnection connection)
    {
        return new DigifinexQuery<DigifinexSubscriptionResult>("all_ticker.subscribe", Array.Empty<object>(), Authenticated);
    }

    public override CryptoExchange.Net.Sockets.Query? GetUnsubQuery()
    {
        return new DigifinexQuery<DigifinexSubscriptionResult>("all_ticker.unsubscribe", Array.Empty<object>(), Authenticated);
    }
}