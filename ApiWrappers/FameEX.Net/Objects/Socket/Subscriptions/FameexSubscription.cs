using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace FameEX.Net.Objects.Socket.Subscriptions
{
    internal class FameexSubscription<T> : Subscription<JToken, JToken>
    {
        private readonly Action<DataEvent<T>> _handler;
        private readonly string _channel;
        protected readonly ILogger _logger;
        private readonly object _subRequest;
        private readonly object _unsubRequest;

        public override HashSet<string> ListenerIdentifiers { get; set; }

        public FameexSubscription(ILogger logger, List<string> symbols, string topic, string channel, Action<DataEvent<T>> handler, bool authenticated = false)
            : base(logger, authenticated)
        {
            _logger = logger;
            _handler = handler;
            _channel = channel;

            ListenerIdentifiers = new HashSet<string> { channel };

            _subRequest = new
            {
                @event = "sub",
                @params = new
                {
                    channel = _channel,
                    cb_id = "1"
                }
            };

            _unsubRequest = new
            {
                @event = "unsub",
                @params = new
                {
                    channel = _channel
                }
            };
        }

        public override Type? GetMessageType(IMessageAccessor message)
        {
            return typeof(JToken);
        }

        public override CryptoExchange.Net.Sockets.Query GetSubQuery(SocketConnection connection)
        {
            return new FameexQuery(_subRequest, false);
        }

        public override CryptoExchange.Net.Sockets.Query? GetUnsubQuery()
        {
            return new FameexQuery(_unsubRequest, false);
        }

        public override CallResult DoHandleMessage(SocketConnection connection, DataEvent<object> message)
        {
            if (message.Data is JToken token)
            {
                try
                {
                    var tick = token["tick"];
                    if (tick != null)
                    {
                        var data = tick.ToObject<T>();
                        if (data != null)
                        {
                            _handler.Invoke(message.As(data));
                        }
                    }
                    return new CallResult(null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse message");
                    return new CallResult(new ServerError("Failed to parse message"));
                }
            }
            return new CallResult(new ServerError("Invalid message format"));
        }
    }

    // Custom Query implementation for FameEX
    internal class FameexQuery : CryptoExchange.Net.Sockets.Query
    {
        public override HashSet<string> ListenerIdentifiers { get; set; } = new HashSet<string>();

        public FameexQuery(object request, bool authenticated) : base(request, authenticated, 1)
        {
        }

        public override Type? GetMessageType(IMessageAccessor message)
        {
            return typeof(JToken);
        }

        public override void Timeout()
        {
            if (Completed)
                return;
            Completed = true;
            Result = new CallResult(new CancellationRequestedError());
            ContinueAwaiter?.Set();
            _event.Set();
        }

        public override void Fail(Error error)
        {
            Result = new CallResult(error);
            Completed = true;
            ContinueAwaiter?.Set();
            _event.Set();
        }

        public override async Task<CallResult> Handle(SocketConnection connection, DataEvent<object> message)
        {
            if (Completed)
                return new CallResult(new ServerError("Query already completed"));

            Completed = true;
            Response = message.Data;
            Result = new CallResult(null);
            ContinueAwaiter?.Set();
            _event.Set();

            return Result;
        }
    }
}