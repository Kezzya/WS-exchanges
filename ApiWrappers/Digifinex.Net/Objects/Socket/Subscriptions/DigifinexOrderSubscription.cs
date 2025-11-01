using CryptoExchange.Net.Objects.Sockets;
using Digifinex.Net.Objects.Models.Socket;
using Microsoft.Extensions.Logging;

namespace Digifinex.Net.Objects.Socket.Subscriptions;

internal class DigifinexOrderSubscription : DigifinexSubscription<List<DigifinexOrder>>
{
    public DigifinexOrderSubscription(ILogger logger, List<string> symbols, Action<DataEvent<List<DigifinexOrder>>> handler, bool authenticated)
        : base(logger, symbols, "order", "order.update", handler, authenticated)
    {
    }
}