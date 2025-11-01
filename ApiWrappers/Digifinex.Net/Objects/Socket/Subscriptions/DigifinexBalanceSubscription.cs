using CryptoExchange.Net.Objects.Sockets;
using Digifinex.Net.Objects.Models.Socket;
using Microsoft.Extensions.Logging;

namespace Digifinex.Net.Objects.Socket.Subscriptions;

internal class DigifinexBalanceSubscription : DigifinexSubscription<List<DigifinexBalance>>
{
    public DigifinexBalanceSubscription(ILogger logger, List<string> currencies, Action<DataEvent<List<DigifinexBalance>>> handler, bool authenticated)
        : base(logger, currencies, "balance", "balance.update", handler, authenticated)
    {
    }
}