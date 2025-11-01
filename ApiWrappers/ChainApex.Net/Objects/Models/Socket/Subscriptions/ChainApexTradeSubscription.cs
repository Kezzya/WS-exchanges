using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace ChainApex.Net.Objects.Models.Socket.Subscriptions
{
    internal class ChainApexTradeSubscription : ChainApexSubscription<ChainApexSocketTradeData>
    {
        private readonly Action<DataEvent<List<ChainApexSocketTrade>>> _typedHandler;

        public ChainApexTradeSubscription(ILogger logger, string symbol, Action<DataEvent<List<ChainApexSocketTrade>>> handler)
            : base(logger, symbol, "trade_ticker", data => { })
        {
            _typedHandler = handler;
        }

        public override CallResult DoHandleMessage(SocketConnection connection, DataEvent<object> message)
        {
            var data = (ChainApexSocketResponse<ChainApexSocketTradeData>)message.Data;
            if (data.Tick?.Data != null)
            {
                _typedHandler.Invoke(message.As(data.Tick.Data, _channel));
            }
            return new CallResult(null);
        }
    }
}

