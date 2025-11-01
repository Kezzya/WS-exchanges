using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.JsonNet;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using ChainApex.Net.Objects.Options;

namespace ChainApex.Net.Clients.SpotApi
{
    public class ChainApexRestClientSpotApi : RestApiClient
    {
        internal static TimeSyncState _timeSyncState = new TimeSyncState("Spot Api");

        public string ExchangeName => "ChainApex";

        public ChainApexRestClientSpotApiExchangeData ExchangeData { get; set; }
        public ChainApexRestClientSpotApiAccount Account { get; set; }

        internal ChainApexRestClientSpotApi(ILogger logger, HttpClient? httpClient, ChainApexRestOptions options) 
            : base(logger, httpClient, options.Environment.RestClientAddress, options, options.SpotOptions)
        {
            ExchangeData = new ChainApexRestClientSpotApiExchangeData(this);
            Account = new ChainApexRestClientSpotApiAccount(this);
            RequestBodyFormat = RequestBodyFormat.Json;
        }

        /// <inheritdoc />
        protected override IMessageSerializer CreateSerializer() => new JsonNetMessageSerializer();
        
        /// <inheritdoc />
        protected override IStreamMessageAccessor CreateAccessor() => new JsonNetStreamMessageAccessor();

        public override TimeSyncInfo? GetTimeSyncInfo() => new TimeSyncInfo(_logger, (ApiOptions.AutoTimestamp ?? ClientOptions.AutoTimestamp), (ApiOptions.TimestampRecalculationInterval ?? ClientOptions.TimestampRecalculationInterval), _timeSyncState);

        public override TimeSpan? GetTimeOffset() => _timeSyncState.TimeOffset;

        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
          => new ChainApexAuthenticationProvider(credentials);

        internal Uri GetUri(string path) => new Uri(BaseAddress.AppendPath(path));

        internal Task<WebCallResult> SendAsync(RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null)
            => SendToAddressAsync(BaseAddress, definition, parameters, cancellationToken, weight);

        internal async Task<WebCallResult> SendToAddressAsync(string baseAddress, RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null)
        {
            // DEBUG: Print request details before sending
            Console.WriteLine($"[DEBUG] ChainApexRestClientSpotApi.SendToAddressAsync - BaseAddress: {baseAddress}");
            Console.WriteLine($"[DEBUG] ChainApexRestClientSpotApi.SendToAddressAsync - Path: {definition.Path}");
            Console.WriteLine($"[DEBUG] ChainApexRestClientSpotApi.SendToAddressAsync - Method: {definition.Method}");
            Console.WriteLine($"[DEBUG] ChainApexRestClientSpotApi.SendToAddressAsync - Full URL will be: {baseAddress.TrimEnd('/')}{definition.Path}");
            
            return await base.SendAsync(baseAddress, definition, parameters, cancellationToken, null, null).ConfigureAwait(false);
        }

        internal Task<WebCallResult<T>> SendAsync<T>(RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null, Dictionary<string, string>? additionalHeaders = null) where T : class
            => SendToAddressAsync<T>(BaseAddress, definition, parameters, cancellationToken, weight, additionalHeaders);

        internal async Task<WebCallResult<T>> SendToAddressAsync<T>(string baseAddress, RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null, Dictionary<string, string>? additionalHeaders = null) where T : class
        {
            // DEBUG: Print request details before sending
            Console.WriteLine($"[DEBUG] ChainApexRestClientSpotApi.SendToAddressAsync<T> - BaseAddress: {baseAddress}");
            Console.WriteLine($"[DEBUG] ChainApexRestClientSpotApi.SendToAddressAsync<T> - Path: {definition.Path}");
            Console.WriteLine($"[DEBUG] ChainApexRestClientSpotApi.SendToAddressAsync<T> - Method: {definition.Method}");
            Console.WriteLine($"[DEBUG] ChainApexRestClientSpotApi.SendToAddressAsync<T> - Full URL will be: {baseAddress.TrimEnd('/')}{definition.Path}");
            
            var result = await base.SendAsync<T>(baseAddress, definition, parameters, cancellationToken, additionalHeaders, null).ConfigureAwait(false);
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
            
            if (code == null || code == 0)
                return null;

            return new ServerError(code.Value, msg ?? "Unknown error");
        }

        public override string FormatSymbol(string baseAsset, string quoteAsset) => $"{baseAsset.ToUpperInvariant()}{quoteAsset.ToUpperInvariant()}";

        public string GetSymbolName(string baseAsset, string quoteAsset) => FormatSymbol(baseAsset, quoteAsset);

        public void SetApiCredentials(ApiCredentials credentials)
        {
            ExchangeData.SetApiCredentials(credentials);
            Account.SetApiCredentials(credentials);
        }
    }
}
