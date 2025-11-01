using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;

namespace ChainApex.Net.Objects.Models.Socket.Subscriptions
{
    internal class ChainApexDepthSubscription : ChainApexSubscription<ChainApexSocketDepth>
    {
        public ChainApexDepthSubscription(ILogger logger, string symbol, Action<DataEvent<ChainApexSocketDepth>> handler)
            : base(logger, symbol, "depth_step0", handler)
        {
        }
    }
}

