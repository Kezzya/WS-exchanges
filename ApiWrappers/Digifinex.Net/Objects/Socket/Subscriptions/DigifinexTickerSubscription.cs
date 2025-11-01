using CryptoExchange.Net.Objects.Sockets;
using Digifinex.Net.Objects.Models.Socket;
using Microsoft.Extensions.Logging;

namespace Digifinex.Net.Objects.Socket.Subscriptions;

internal class DigifinexTickerSubscription : DigifinexSubscription<List<DigifinexTicker>>
{
    public DigifinexTickerSubscription(ILogger logger, List<string> symbols, Action<DataEvent<List<DigifinexTicker>>> handler)
        : base(logger, symbols, "ticker", "ticker.update", handler, false)
    {
    }
}