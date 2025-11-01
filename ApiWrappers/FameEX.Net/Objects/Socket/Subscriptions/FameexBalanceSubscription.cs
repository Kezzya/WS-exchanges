using CryptoExchange.Net.Objects.Sockets;
using FameEX.Net.Objects.Models.Socket;
using Microsoft.Extensions.Logging;

namespace FameEX.Net.Objects.Socket.Subscriptions
{
    internal class FameexBalanceSubscription : FameexSubscription<List<FameexBalance>>
    {
        public FameexBalanceSubscription(ILogger logger, List<string> currencies, Action<DataEvent<List<FameexBalance>>> handler, bool authenticated)
            : base(logger, currencies, "balance", "balance.update", handler, authenticated)
        {
        }
    }
}