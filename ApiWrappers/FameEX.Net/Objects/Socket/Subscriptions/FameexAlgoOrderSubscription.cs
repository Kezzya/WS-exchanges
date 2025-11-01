using CryptoExchange.Net.Objects.Sockets;
using FameEX.Net.Objects.Models.Socket;
using Microsoft.Extensions.Logging;

namespace FameEX.Net.Objects.Socket.Subscriptions
{
    internal class FameexAlgoOrderSubscription : FameexSubscription<List<FameexAlgoOrder>>
    {
        public FameexAlgoOrderSubscription(ILogger logger, List<string> symbols, Action<DataEvent<List<FameexAlgoOrder>>> handler, bool authenticated)
            : base(logger, symbols, "order_algo", "order_algo.update", handler, authenticated)
        {
        }
    }
}