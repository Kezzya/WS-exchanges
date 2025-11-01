using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using FameEX.Net.Objects.Models.Socket;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace FameEX.Net.Objects.Socket.Subscriptions
{
    internal class FameexDepthSubscription : FameexSubscription<FameexDepthUpdate>
    {
        private readonly Action<DataEvent<FameexDepthUpdate>> _typedHandler;

        public FameexDepthSubscription(ILogger logger, List<string> symbols, Action<DataEvent<FameexDepthUpdate>> handler)
            : base(logger, symbols, "depth_step0", $"market_{symbols[0].ToLower().Replace("usdt", "_usdt")}_depth_step0", handler, false)
        {
            _typedHandler = handler;
        }

        public override CallResult DoHandleMessage(SocketConnection connection, DataEvent<object> message)
        {
            var socketUpdate = message.Data as FameexSocketResponse<FameexDepthUpdate>;
            if (socketUpdate == null)
            {
                _logger.LogError("Invalid message format: expected FameexSocketResponse<FameexDepthUpdate>");
                return new CallResult(new ServerError("Invalid message format"));
            }

            _typedHandler.Invoke(message.As(socketUpdate.Tick));
            return new CallResult(null);
        }

        public override Type? GetMessageType(IMessageAccessor message) => typeof(FameexSocketResponse<FameexDepthUpdate>);
    }

    // Остальной код FameexSubscription и других подписок остается как в предыдущем ответе
}