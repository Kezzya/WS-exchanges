using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.Socket.Subscriptions
{
    /// <summary>
    /// ChainStream alternative WebSocket subscription
    /// Uses format: {"type":"subscribe","channel":"..."}
    /// </summary>
    internal abstract class ChainStreamSubscription<T> : Subscription<ChainStreamResponse<T>, ChainStreamResponse<T>>
    {
        protected readonly Action<DataEvent<T>> _handler;
        protected readonly string _channel;
        protected readonly string? _token;

        public override HashSet<string> ListenerIdentifiers { get; set; }

        protected ChainStreamSubscription(ILogger logger, string channel, Action<DataEvent<T>> handler, string? token = null)
            : base(logger, false)
        {
            _handler = handler;
            _channel = channel;
            _token = token;
            ListenerIdentifiers = new HashSet<string> { channel };
        }

        public override Query? GetSubQuery(SocketConnection connection)
        {
            var request = new ChainStreamRequest
            {
                Type = "subscribe",
                Channel = _channel
            };
            
            var json = JsonConvert.SerializeObject(request);
            _logger.LogDebug($"[DEBUG ChainStream] Subscription request JSON: {json}");
            _logger.LogDebug($"[DEBUG ChainStream] Channel: {_channel}");
            
            return new ChainStreamQuery<T>(request, _channel);
        }

        public override Query? GetUnsubQuery()
        {
            var request = new ChainStreamRequest
            {
                Type = "unsubscribe",
                Channel = _channel
            };
            
            return new ChainStreamQuery<T>(request, _channel);
        }

        public override CallResult DoHandleMessage(SocketConnection connection, DataEvent<object> message)
        {
            var data = (ChainStreamResponse<T>)message.Data;
            if (data.Data != null)
            {
                _handler.Invoke(message.As(data.Data, _channel));
            }
            return new CallResult(null);
        }

        public override Type? GetMessageType(IMessageAccessor message) => typeof(ChainStreamResponse<T>);
    }

    internal class ChainStreamQuery<T> : Query<ChainStreamResponse<T>>
    {
        public override HashSet<string> ListenerIdentifiers { get; set; }

        public ChainStreamQuery(ChainStreamRequest request, string channel, int weight = 1)
            : base(request, false, weight)
        {
            ListenerIdentifiers = new HashSet<string> { channel };
        }

        public override CallResult<ChainStreamResponse<T>> HandleMessage(SocketConnection connection, DataEvent<ChainStreamResponse<T>> message)
        {
            return new CallResult<ChainStreamResponse<T>>(message.Data);
        }
    }
}

