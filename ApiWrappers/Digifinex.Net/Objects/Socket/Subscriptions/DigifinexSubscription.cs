using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Digifinex.Net.Objects.Models.Socket;
using Digifinex.Net.Objects.Socket.Query;
using Microsoft.Extensions.Logging;

namespace Digifinex.Net.Objects.Socket.Subscriptions
{
    internal abstract class DigifinexSubscription<T> : Subscription<DigifinexSocketResponse<DigifinexSubscriptionResult>, DigifinexSocketResponse<DigifinexSubscriptionResult>>
    {
        protected readonly Action<DataEvent<T>> _handler;
        protected readonly List<string> _symbols;
        protected readonly string _method;
        protected readonly string _updateMethod;

        public override HashSet<string> ListenerIdentifiers { get; set; }

        protected DigifinexSubscription(ILogger logger, List<string> symbols, string method, string updateMethod, Action<DataEvent<T>> handler, bool authenticated)
            : base(logger, authenticated)
        {
            _handler = handler;
            _symbols = symbols;
            _method = method;
            _updateMethod = updateMethod;
            ListenerIdentifiers = new HashSet<string> { updateMethod };
        }

        public override CryptoExchange.Net.Sockets.Query? GetSubQuery(SocketConnection connection)
        {
            return new DigifinexQuery<DigifinexSubscriptionResult>($"{_method}.subscribe", _symbols.ToArray(), Authenticated);
        }

        public override CryptoExchange.Net.Sockets.Query? GetUnsubQuery()
        {
            return new DigifinexQuery<DigifinexSubscriptionResult>($"{_method}.unsubscribe", _symbols.ToArray(), Authenticated);
        }

        public override CallResult DoHandleMessage(SocketConnection connection, DataEvent<object> message)
        {
            var data = (DigifinexSocketUpdate<T>)message.Data;
            _handler.Invoke(message.As(data.Params));
            return new CallResult(null);
        }

        public override Type? GetMessageType(IMessageAccessor message) => typeof(DigifinexSocketUpdate<T>);
    }
}