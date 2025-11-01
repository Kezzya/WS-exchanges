using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.Socket.Subscriptions
{
    internal abstract class ChainApexSubscription<T> : Subscription<ChainApexSocketResponse<T>, ChainApexSocketResponse<T>>
    {
        protected readonly Action<DataEvent<T>> _handler;
        protected readonly string _symbol;
        protected readonly string _channel;

        public override HashSet<string> ListenerIdentifiers { get; set; }

        protected ChainApexSubscription(ILogger logger, string symbol, string channelType, Action<DataEvent<T>> handler)
            : base(logger, false)
        {
            _handler = handler;
            _symbol = symbol.ToLowerInvariant();
            _channel = $"market_{_symbol}_{channelType}";
            ListenerIdentifiers = new HashSet<string> { _channel };
        }

        public override Query? GetSubQuery(SocketConnection connection)
        {
            var request = new ChainApexSocketRequest
            {
                Event = "sub",
                Params = new ChainApexSocketRequestParams
                {
                    Channel = _channel,
                    CbId = Guid.NewGuid().ToString("N").Substring(0, 8)
                }
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);
            _logger.LogDebug($"[DEBUG WebSocket] Subscription request JSON: {json}");
            _logger.LogDebug($"[DEBUG WebSocket] Channel identifier: {_channel}");
            
            return new ChainApexQuery(request, _channel);
        }

        public override Query? GetUnsubQuery()
        {
            var request = new ChainApexSocketRequest
            {
                Event = "unsub",
                Params = new ChainApexSocketRequestParams
                {
                    Channel = _channel,
                    CbId = Guid.NewGuid().ToString("N").Substring(0, 8)
                }
            };
            
            return new ChainApexQuery(request, _channel);
        }

        public override CallResult DoHandleMessage(SocketConnection connection, DataEvent<object> message)
        {
            var data = (ChainApexSocketResponse<T>)message.Data;
            if (data.Tick != null)
            {
                _handler.Invoke(message.As(data.Tick, _channel));
            }
            return new CallResult(null);
        }

        public override Type? GetMessageType(IMessageAccessor message) => typeof(ChainApexSocketResponse<T>);
    }
}

