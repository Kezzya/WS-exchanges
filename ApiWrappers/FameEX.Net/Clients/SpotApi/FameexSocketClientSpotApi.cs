using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using FameEX.Net.Objects.Models.Socket;
using FameEX.Net.Objects.Options;
using FameEX.Net.Objects.Socket.Query;
using FameEX.Net.Objects.Socket.Subscriptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FameEX.Net.Clients.SpotApi
{
    public class FameexSocketClientSpotApi : SocketApiClient
    {
        private static readonly MessagePath _channelPath = MessagePath.Get().Property("channel");
        private static readonly MessagePath _pongPath = MessagePath.Get().Property("pong");
        private ApiCredentials? _credentials;

        public FameexSocketClientSpotApi(ILogger logger, FameexSocketOptions options)
            : base(logger, options.Environment.PublicSocketClientAddress, options, options.SpotOptions ?? new SocketApiOptions())
        {
            RegisterPeriodicQuery("server.ping", TimeSpan.FromSeconds(25), x => new FameexPingQuery(), x =>
            {
                var pong = x.Success;
                _logger.LogDebug($"Received pong: {pong}");
            });
            options.OutputOriginalData = false;
            _credentials = options.ApiCredentials;
        }

        public override string FormatSymbol(string baseAsset, string quoteAsset) => $"{baseAsset.ToLower()}{quoteAsset.ToLower()}";

        public override string? GetListenerIdentifier(IMessageAccessor message)
        {
            if (message.IsJson)
            {
                var pongValue = message.GetValue<long?>(_pongPath);
                if (pongValue.HasValue)
                    return "pong";

                var channelValue = message.GetValue<string>(_channelPath);
                return channelValue ?? null;
            }
            else if (message is CryptoExchange.Net.Converters.JsonNet.JsonNetByteMessageAccessor byteAccessor)
            {
                // Получаем исходные байты
                var bytes = byteAccessor.GetOriginalBytes();
                try
                {
                    using var ms = new MemoryStream(bytes);
                    using var gzip = new GZipStream(ms, CompressionMode.Decompress);
                    using var reader = new StreamReader(gzip, Encoding.UTF8);
                    var decompressed = reader.ReadToEnd();

                    var jToken = JToken.Parse(decompressed);
                    var channel = jToken["channel"]?.ToString();
                    if (channel != null)
                        return channel;
                    return null;
                }
                catch
                {
                    // Если не удалось декомпрессировать или распарсить, возвращаем base64 для отладки
                    return Convert.ToBase64String(bytes);
                }
            }
            else
            {
                // fallback для других реализаций IMessageAccessor
                return message.GetOriginalString();
            }
        }
        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
        {
            return new FameexAuthenticationProvider(credentials);
        }

        public void SetApiCredentials(ApiCredentials credentials)
        {
            _credentials = credentials;
        }

        public async Task<CallResult<UpdateSubscription>> SubscribeToTickerAsync(
            List<string> symbols,
            Action<DataEvent<FameexTicker>> handler,
            CancellationToken ct = default)
        {
            var subscription = new FameexTickerSubscription(_logger, symbols, handler);
            _logger.LogDebug($"Sending ticker subscription request: {JsonConvert.SerializeObject(new FameexSocketRequest { Event = "sub", Params = new FameexSocketRequestParams { Channel = $"market_{symbols[0].ToLower()}_ticker", CallbackId = "1" } })}");
            return await SubscribeAsync(subscription, ct).ConfigureAwait(false);
        }
        public async Task<CallResult<UpdateSubscription>> SubscribeToTradesAsync(
    List<string> symbols,
    Action<DataEvent<List<FameexTrade>>> handler,
    CancellationToken ct = default)
        {
            if (symbols == null || symbols.Count == 0)
                throw new ArgumentException("Symbols cannot be null or empty");

            var subscription = new FameexTradesSubscription(_logger, symbols, handler);
            return await SubscribeAsync(BaseAddress, subscription, ct).ConfigureAwait(false);
        }
        public async Task<CallResult<UpdateSubscription>> SubscribeToOrderBookAsync(
    List<string> symbols,
    int depth,
    Action<DataEvent<FameexOrderBook>> handler,
    CancellationToken ct = default)
        {
            if (symbols == null || symbols.Count == 0)
            {
                _logger.LogError("Symbols cannot be null or empty");
                throw new ArgumentException("Symbols cannot be null or empty");
            }
                

            var subscription = new FameexOrderBookSubscription(_logger, symbols, depth, handler);
            return await SubscribeAsync(BaseAddress, subscription, ct).ConfigureAwait(false);
        }

         
        

       
        protected override Query GetAuthenticationRequest(SocketConnection connection)
        {
            if (_credentials == null)
            {
                _logger.LogError("No credentials provided!");
                throw new InvalidOperationException("No credentials provided");
            }

            CreateAuthenticationProvider(_credentials);

            var apiKey = _credentials.Key.GetString();

            Console.WriteLine($"[AUTH] Creating auth request with ApiKey: {apiKey}");
            _logger.LogInformation($"[AUTH] Creating auth request with ApiKey: {apiKey}");

            // ВАРИАНТ 1: Только ApiKey
            return new FameexAuthQuery(apiKey);

            // ВАРИАНТ 2: С timestamp и signature (если Вариант 1 не работает)
            /*
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var authProvider = (FameexAuthenticationProvider)AuthenticationProvider!;
            var sign = authProvider.CreateWebsocketSignature(timestamp);

            Console.WriteLine($"[AUTH] Timestamp: {timestamp}, Sign: {sign}");
            _logger.LogInformation($"[AUTH] Timestamp: {timestamp}, Sign: {sign}");

            return new FameexAuthQuery(apiKey, timestamp, sign);
            */
        }

        



        public async Task<CallResult<UpdateSubscription>> SubscribeToOrderUpdatesAsync(
            Action<DataEvent<FameexOrderUpdate>> handler,
            CancellationToken ct = default)
        {
            if (_credentials == null)
            {
                _logger.LogDebug("No API credentials provided, cannot subscribe to order updates");
                throw new InvalidOperationException("API credentials required for order updates subscription");
            }
               

            var subscription = new FameexOrderSubscription(_logger, handler);
            _logger.LogDebug($"Sending order updates subscription request: {JsonConvert.SerializeObject(new FameexSocketRequest { Event = "sub", Params = new FameexSocketRequestParams { Channel = "order", CallbackId = "1" } })}");
            return await SubscribeAsync(subscription, ct).ConfigureAwait(false);
        }
        //    public async Task<CallResult<UpdateSubscription>> SubscribeToOrderUpdatesAsync(
        //IEnumerable<string> symbols,
        //Action<DataEvent<FameexOrderUpdate>> handler,
        //CancellationToken ct = default)
        //    {
        //        if (_credentials == null)
        //        {
        //            _logger.LogDebug("No API credentials provided, cannot subscribe to order updates");
        //            throw new InvalidOperationException("API credentials required for order updates subscription");
        //        }

        //        var subscriptions = new List<FameexOrderSubscription>();
        //        foreach (var symbol in symbols)
        //        {
        //            var sub = new FameexOrderSubscription(_logger, handler, symbol);
        //            subscriptions.Add(sub);
        //        }

        //        //var combinedSub = new CombinedSubscription(subscriptions);
        //        var combinedSub = new CombinedSubscription("test");
        //        //_logger.LogDebug($"Sending order updates subscription requests for symbols: {string.Join(", ", symbols)}");

        //        //// Authenticate first if not done
        //        //// Assume auth is handled separately or in base

        //        //foreach (var symbol in symbols)
        //        //{
        //        //    var request = new FameexSocketRequest
        //        //    {
        //        //        Event = "sub",
        //        //        Topic = "spot.orders",
        //        //        Params = new FameexSocketRequestParams { Symbol = symbol.ToUpperInvariant() }
        //        //    };
        //        //    await SubscribeAsync(combinedSub, request, ct).ConfigureAwait(false);
        //        //}

        //        //return new CallResult<UpdateSubscription>(combinedSub);
        //        return new CallResult<UpdateSubscription>(new CombinedSubscription("test"));
        //    }
        public async Task<CallResult<UpdateSubscription>> SubscribeToOrderUpdatesAsync(
        IEnumerable<string> symbols,
        Action<DataEvent<FameexOrderUpdate>> handler,
        CancellationToken ct = default)
        {
            if (_credentials == null)
            {
                _logger.LogDebug("No API credentials provided, cannot subscribe to order updates");
                throw new InvalidOperationException("API credentials required for order updates subscription");
            }

            // Создаем заглушку UpdateSubscription
            var stubSubscription = new UpdateSubscription(null, null); // Минимальная заглушка
            return new CallResult<UpdateSubscription>(stubSubscription);
        }
    }

}