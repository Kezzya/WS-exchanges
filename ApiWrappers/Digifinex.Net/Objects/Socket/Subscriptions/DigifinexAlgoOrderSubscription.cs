using CryptoExchange.Net.Objects.Sockets;
using Digifinex.Net.Objects.Models.Socket;
using Microsoft.Extensions.Logging;

namespace Digifinex.Net.Objects.Socket.Subscriptions;

internal class DigifinexAlgoOrderSubscription : DigifinexSubscription<List<DigifinexAlgoOrder>>
{
    public DigifinexAlgoOrderSubscription(ILogger logger, List<string> symbols, Action<DataEvent<List<DigifinexAlgoOrder>>> handler, bool authenticated)
        : base(logger, symbols, "order_algo", "order_algo.update", handler, authenticated)
    {
    }
}