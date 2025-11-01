using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using ChainApex.Net.Objects.Options;
using ChainApex.Net.Objects.Models.Socket;
using ChainApex.Net.Objects.Models.Socket.Subscriptions;
using Newtonsoft.Json;
using System.IO.Compression;

namespace ChainApex.Net.Clients.SpotApi
{
    public class ChainApexSocketClientSpotApi : SocketApiClient
    {
        private static readonly MessagePath _channelPath = MessagePath.Get().Property("channel");
        private static readonly MessagePath _pingPath = MessagePath.Get().Property("ping");
        private static readonly MessagePath _pongPath = MessagePath.Get().Property("pong");
        private ApiCredentials? _credentials;

        public ChainApexSocketClientSpotApi(ILogger logger, string baseAddress, ChainApexSocketOptions options) 
            : base(logger, baseAddress, options, options.SpotOptions)
        {
        }

        public void SetApiCredentials(ApiCredentials credentials)
        {
            _credentials = credentials;
        }

        public override string FormatSymbol(string baseAsset, string quoteAsset) => $"{baseAsset.ToUpperInvariant()}{quoteAsset.ToUpperInvariant()}";

        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
        {
            return new ChainApexAuthenticationProvider(credentials);
        }

        public override string? GetListenerIdentifier(IMessageAccessor messageAccessor)
        {
            if (!messageAccessor.IsJson)
            {
                return messageAccessor.GetOriginalString();
            }

            // Check for ping/pong
            var ping = messageAccessor.GetValue<long?>(_pingPath);
            if (ping != null)
            {
                return "ping";
            }

            var pong = messageAccessor.GetValue<long?>(_pongPath);
            if (pong != null)
            {
                return "pong";
            }

            // Check for channel
            var channel = messageAccessor.GetValue<string>(_channelPath);
            if (channel != null)
            {
                return channel;
            }

            return null;
        }

        protected override Query? GetAuthenticationRequest(SocketConnection connection)
        {
            // Try ChainStream authentication if credentials are available
            if (_credentials != null)
            {
                var authRequest = new ChainStreamRequest
                {
                    Type = "auth",
                    Token = _credentials.Key.GetString()
                };
                
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(authRequest);
                _logger.LogDebug($"[DEBUG ChainStream] Sending authentication: {json}");
                
                return new ChainStreamQuery<object>(authRequest, "auth");
            }
            
            // ChainApex original protocol doesn't require authentication for WebSocket connections
            return null;
        }

        /// <summary>
        /// Subscribe to depth updates
        /// </summary>
        public async Task<CallResult<UpdateSubscription>> SubscribeToDepthUpdatesAsync(
            string symbol,
            Action<DataEvent<ChainApexSocketDepth>> onMessage,
            CancellationToken ct = default)
        {
            _logger.LogDebug($"[DEBUG WebSocket] Subscribing to depth updates");
            _logger.LogDebug($"[DEBUG WebSocket] BaseAddress: {BaseAddress}");
            _logger.LogDebug($"[DEBUG WebSocket] Symbol: {symbol}");
            _logger.LogDebug($"[DEBUG WebSocket] Channel: market_{symbol.ToLowerInvariant()}_depth_step0");
            
            var subscription = new ChainApexDepthSubscription(_logger, symbol, onMessage);
            return await SubscribeAsync(BaseAddress, subscription, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Subscribe to trade updates
        /// </summary>
        public async Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(
            string symbol,
            Action<DataEvent<List<ChainApexSocketTrade>>> onMessage,
            CancellationToken ct = default)
        {
            var subscription = new ChainApexTradeSubscription(_logger, symbol, onMessage);
            return await SubscribeAsync(BaseAddress, subscription, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Subscribe to ticker updates
        /// </summary>
        public async Task<CallResult<UpdateSubscription>> SubscribeToTickerUpdatesAsync(
            string symbol,
            Action<DataEvent<ChainApexSocketTicker>> onMessage,
            CancellationToken ct = default)
        {
            var subscription = new ChainApexTickerSubscription(_logger, symbol, onMessage);
            return await SubscribeAsync(BaseAddress, subscription, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Subscribe to kline updates
        /// </summary>
        public async Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(
            string symbol,
            string interval,
            Action<DataEvent<ChainApexSocketKline>> onMessage,
            CancellationToken ct = default)
        {
            var subscription = new ChainApexKlineSubscription(_logger, symbol, interval, onMessage);
            return await SubscribeAsync(BaseAddress, subscription, ct).ConfigureAwait(false);
        }

        #region ChainStream Alternative Protocol
        
        /// <summary>
        /// Subscribe using ChainStream protocol format (alternative)
        /// Uses format: {"type":"subscribe","channel":"dex-..."}
        /// </summary>
        public async Task<CallResult<UpdateSubscription>> SubscribeChainStreamAsync<T>(
            string channel,
            Action<DataEvent<T>> onMessage,
            CancellationToken ct = default)
        {
            _logger.LogInformation($"[DEBUG ChainStream] Using alternative ChainStream protocol");
            _logger.LogInformation($"[DEBUG ChainStream] Channel: {channel}");
            
            var subscription = new ChainStreamGenericSubscription<T>(_logger, channel, onMessage);
            return await SubscribeAsync(BaseAddress, subscription, ct).ConfigureAwait(false);
        }

        #endregion
    }

    /// <summary>
    /// Generic ChainStream subscription
    /// </summary>
    internal class ChainStreamGenericSubscription<T> : ChainStreamSubscription<T>
    {
        public ChainStreamGenericSubscription(ILogger logger, string channel, Action<DataEvent<T>> handler, string? token = null)
            : base(logger, channel, handler, token)
        {
        }
    }
}
