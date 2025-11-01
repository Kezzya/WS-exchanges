using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Converters.JsonNet;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.CommonClients;
using CryptoExchange.Net.Objects;
using Digifinex.Net.Objects.Options;
using Microsoft.Extensions.Logging;
using OrderBook = CryptoExchange.Net.CommonObjects.OrderBook;

namespace Digifinex.Net.Clients.SpotApi
{
    public class DigifinexRestClientSpotApi : RestApiClient, ISpotClient
    {
        internal static TimeSyncState _timeSyncState = new TimeSyncState("Spot Api");

        public string ExchangeName => "Digifinex";

        /// <inheritdoc />
        public event Action<OrderId>? OnOrderPlaced;

        /// <inheritdoc />
        public event Action<OrderId>? OnOrderCanceled;

        public DigifinexRestClientSpotApiExchangeData ExchangeData { get; set; }
        public DigifinexRestClientSpotApiAccount Account { get; set; }

        public DigifinexRestClientSpotApi(ILogger logger, HttpClient? httpClient, DigifinexRestOptions options)
         : base(logger, httpClient, options.Environment.RestClientAddress, options, options.SpotOptions)
        {
            ExchangeData = new DigifinexRestClientSpotApiExchangeData(this);
            Account = new DigifinexRestClientSpotApiAccount(this);
            RequestBodyFormat = RequestBodyFormat.FormData;

            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        }

        /// <inheritdoc />
        protected override IMessageSerializer CreateSerializer() => new JsonNetMessageSerializer();
        
        /// <inheritdoc />
        protected override IStreamMessageAccessor CreateAccessor() => new JsonNetStreamMessageAccessor();

        public override TimeSyncInfo? GetTimeSyncInfo() => new TimeSyncInfo(_logger, (ApiOptions.AutoTimestamp ?? ClientOptions.AutoTimestamp), (ApiOptions.TimestampRecalculationInterval ?? ClientOptions.TimestampRecalculationInterval), _timeSyncState);

        public override TimeSpan? GetTimeOffset() => _timeSyncState.TimeOffset;

        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
          => new DigifinexAuthenticationProvider(credentials);

        internal Uri GetUri(string path) => new Uri(BaseAddress.AppendPath(path));

        internal Task<WebCallResult> SendAsync(RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null)
            => SendToAddressAsync(BaseAddress, definition, parameters, cancellationToken, weight);

        internal async Task<WebCallResult> SendToAddressAsync(string baseAddress, RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null)
        {
            return await base.SendAsync(baseAddress, definition, parameters, cancellationToken, null, weight).ConfigureAwait(false);
        }

        internal Task<WebCallResult<T>> SendAsync<T>(RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null, Dictionary<string, string>? additionalHeaders = null) where T : class
            => SendToAddressAsync<T>(BaseAddress, definition, parameters, cancellationToken, weight, additionalHeaders);

        internal async Task<WebCallResult<T>> SendToAddressAsync<T>(string baseAddress, RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null, Dictionary<string, string>? additionalHeaders = null) where T : class
        {
            var result = await base.SendAsync<T>(baseAddress, definition, parameters, cancellationToken, additionalHeaders, weight).ConfigureAwait(false);
            return result;
        }

        protected override ServerRateLimitError ParseRateLimitResponse(int httpStatusCode, IEnumerable<KeyValuePair<string, IEnumerable<string>>> responseHeaders, IMessageAccessor accessor)
        {
            var retryAfterHeader = responseHeaders.SingleOrDefault(r => r.Key.Equals("Retry-After", StringComparison.InvariantCultureIgnoreCase));
            if (retryAfterHeader.Value?.Any() == true && int.TryParse(retryAfterHeader.Value.First(), out var seconds))
                return new ServerRateLimitError("Rate limit exceeded") { RetryAfter = DateTime.UtcNow.AddSeconds(seconds) };

            return new ServerRateLimitError("Rate limit exceeded") { RetryAfter = DateTime.UtcNow.AddSeconds(10) };
        }

        /// <inheritdoc />
        protected override ServerError? TryParseError(IMessageAccessor accessor)
        {
            var code = accessor.GetValue<int?>(MessagePath.Get().Property("code"));
            var msg = accessor.GetValue<string>(MessagePath.Get().Property("msg"));
            
            if (code == null || code == 200 || code == 0)
                return null;

            return new ServerError(code.Value, msg ?? "Unknown error");
        }

        public override string FormatSymbol(string baseAsset, string quoteAsset) => $"{baseAsset.ToUpperInvariant()}{quoteAsset.ToUpperInvariant()}";

        public string GetSymbolName(string baseAsset, string quoteAsset) => FormatSymbol(baseAsset, quoteAsset);

        // ISpotClient implementation methods
        public Task<WebCallResult<OrderId>> PlaceOrderAsync(string symbol, CommonOrderSide side, CommonOrderType type, decimal quantity, decimal? price = null, string? accountId = null, string? clientOrderId = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebCallResult<IEnumerable<Symbol>>> GetSymbolsAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebCallResult<Ticker>> GetTickerAsync(string symbol, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebCallResult<IEnumerable<Ticker>>> GetTickersAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebCallResult<IEnumerable<Kline>>> GetKlinesAsync(string symbol, TimeSpan timespan, DateTime? startTime = null, DateTime? endTime = null, int? limit = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebCallResult<OrderBook>> GetOrderBookAsync(string symbol, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebCallResult<IEnumerable<Trade>>> GetRecentTradesAsync(string symbol, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebCallResult<IEnumerable<Balance>>> GetBalancesAsync(string? accountId = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebCallResult<Order>> GetOrderAsync(string orderId, string? symbol = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebCallResult<IEnumerable<UserTrade>>> GetOrderTradesAsync(string orderId, string? symbol = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebCallResult<IEnumerable<Order>>> GetOpenOrdersAsync(string? symbol = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebCallResult<IEnumerable<Order>>> GetClosedOrdersAsync(string? symbol = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebCallResult<OrderId>> CancelOrderAsync(string orderId, string? symbol = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}