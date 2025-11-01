using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Digifinex.Net.Objects.Models.Socket;
using Digifinex.Net.Objects.Options;
using Digifinex.Net.Objects.Socket.Subscriptions;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using Digifinex.Net.Objects.Socket.Query;
using DigifinexOrder = Digifinex.Net.Objects.Models.Socket.DigifinexOrder;
using DigifinexServerTime = Digifinex.Net.Objects.Models.Socket.DigifinexServerTime;
using DigifinexTicker = Digifinex.Net.Objects.Models.Socket.DigifinexTicker;

namespace Digifinex.Net.Clients.SpotApi
{
    public class DigifinexSocketClientSpotApi : SocketApiClient
    {
        private static readonly MessagePath _idPath = MessagePath.Get().Property("id");
        private static readonly MessagePath _methodPath = MessagePath.Get().Property("method");
        private ApiCredentials? _credentials;

        public DigifinexSocketClientSpotApi(ILogger logger, DigifinexSocketOptions options)
            : base(logger, options.Environment.PublicSocketClientAddress, options, options.SpotOptions)
        {
            RegisterPeriodicQuery("server.ping", TimeSpan.FromSeconds(30), x => new DigifinexPingQuery(), null);
        }

        public override ReadOnlyMemory<byte> PreprocessStreamMessage(SocketConnection connection, WebSocketMessageType type, ReadOnlyMemory<byte> data)
        {
            return type != WebSocketMessageType.Binary ? data : data.DecompressZlib();
        }

        public override string FormatSymbol(string baseAsset, string quoteAsset) => $"{baseAsset}_{quoteAsset}";

        public override string? GetListenerIdentifier(IMessageAccessor messageAccessor)
        {
            if (!messageAccessor.IsJson)
            {
                return messageAccessor.GetOriginalString();
            }

            var id = messageAccessor.GetValue<int?>(_idPath);
            if (id != null)
            {
                return id.ToString();
            }

            return messageAccessor.GetValue<string>(_methodPath);
        }

        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
        {
            _credentials = credentials;
            return new DigifinexAuthenticationProvider(credentials);
        }

        internal void SetApiCredentials(ApiCredentials credentials)
        {
            _credentials = credentials;
        }

        // Public subscriptions
        public async Task<CallResult<UpdateSubscription>> SubscribeToTradesAsync(
            List<string> symbols,
            Action<DataEvent<DigifinexTradesUpdate>> handler,
            CancellationToken ct = default)
        {
            var subscription = new DigifinexTradesSubscription(_logger, symbols, handler);
            return await SubscribeAsync(BaseAddress, subscription, ct).ConfigureAwait(false);
        }

        public async Task<CallResult<UpdateSubscription>> SubscribeToDepthAsync(
            List<string> symbols,
            Action<DataEvent<DigifinexDepthUpdate>> handler,
            CancellationToken ct = default)
        {
            var subscription = new DigifinexDepthSubscription(_logger, symbols, handler);
            return await SubscribeAsync(BaseAddress, subscription, ct).ConfigureAwait(false);
        }

        public async Task<CallResult<UpdateSubscription>> SubscribeToAllTickerAsync(
            Action<DataEvent<List<DigifinexTicker>>> handler,
            CancellationToken ct = default)
        {
            var subscription = new DigifinexAllTickerSubscription(_logger, handler);
            return await SubscribeAsync(BaseAddress, subscription, ct).ConfigureAwait(false);
        }

        public async Task<CallResult<UpdateSubscription>> SubscribeToTickerAsync(
            List<string> symbols,
            Action<DataEvent<List<DigifinexTicker>>> handler,
            CancellationToken ct = default)
        {
            var subscription = new DigifinexTickerSubscription(_logger, symbols, handler);
            return await SubscribeAsync(BaseAddress, subscription, ct).ConfigureAwait(false);
        }

        // Private subscriptions
        public async Task<CallResult<UpdateSubscription>> SubscribeToOrdersAsync(
            List<string> symbols,
            Action<DataEvent<List<DigifinexOrder>>> handler,
            CancellationToken ct = default)
        {
            var subscription = new DigifinexOrderSubscription(_logger, symbols, handler, true);
            return await SubscribeAsync(BaseAddress, subscription, ct).ConfigureAwait(false);
        }

        public async Task<CallResult<UpdateSubscription>> SubscribeToAlgoOrdersAsync(
            List<string> symbols,
            Action<DataEvent<List<DigifinexAlgoOrder>>> handler,
            CancellationToken ct = default)
        {
            var subscription = new DigifinexAlgoOrderSubscription(_logger, symbols, handler, true);
            return await SubscribeAsync(BaseAddress, subscription, ct).ConfigureAwait(false);
        }

        public async Task<CallResult<UpdateSubscription>> SubscribeToBalanceAsync(
            List<string> currencies,
            Action<DataEvent<List<DigifinexBalance>>> handler,
            CancellationToken ct = default)
        {
            var subscription = new DigifinexBalanceSubscription(_logger, currencies, handler, true);
            return await SubscribeAsync(BaseAddress, subscription, ct).ConfigureAwait(false);
        }

        protected override Query GetAuthenticationRequest(SocketConnection connection)
        {
            if (_credentials == null)
                throw new InvalidOperationException("No credentials provided");

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var authProvider = (DigifinexAuthenticationProvider)AuthenticationProvider!;
            var sign = authProvider.CreateWebsocketSignature(timestamp);

            return new DigifinexAuthQuery(
                _credentials.Key.GetString(),
                timestamp.ToString(),
                sign
            );
        }
    }
}