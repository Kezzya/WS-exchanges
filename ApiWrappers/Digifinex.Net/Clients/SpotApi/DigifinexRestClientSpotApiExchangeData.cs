// DigifinexRestClientSpotApiExchangeData.cs

using CryptoExchange.Net;
using CryptoExchange.Net.Converters.JsonNet;
using CryptoExchange.Net.Objects;
using Digifinex.Net.Objects.Models.ExchangeData;

namespace Digifinex.Net.Clients.SpotApi
{
    public class DigifinexRestClientSpotApiExchangeData
    {
        private readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        private readonly DigifinexRestClientSpotApi _baseClient;

        internal DigifinexRestClientSpotApiExchangeData(DigifinexRestClientSpotApi baseClient)
        {
            _baseClient = baseClient;
        }

        /// <summary>
        /// Test connectivity to the Rest API
        /// </summary>
        public async Task<WebCallResult<DigifinexPing>> PingAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/ping", DigifinexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<DigifinexPing>(request, null, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get server time
        /// </summary>
        public async Task<WebCallResult<DigifinexServerTime>> GetServerTimeAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/time", DigifinexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<DigifinexServerTime>(request, null, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get all market descriptions
        /// </summary>
        public async Task<WebCallResult<DigifinexMarketsResponse>> GetMarketsAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/markets", DigifinexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<DigifinexMarketsResponse>(request, null, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get ticker price
        /// </summary>
        public async Task<WebCallResult<DigifinexTickerResponse>> GetTickerAsync(string? symbol = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("symbol", symbol);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/ticker", DigifinexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<DigifinexTickerResponse>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get order book
        /// </summary>
        public async Task<WebCallResult<DigifinexOrderBook>> GetOrderBookAsync(string symbol, int? limit = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "symbol", symbol }
            };
            parameters.AddOptionalParameter("limit", limit);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/order_book", DigifinexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<DigifinexOrderBook>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get recent trades
        /// </summary>
        public async Task<WebCallResult<DigifinexTradesResponse>> GetRecentTradesAsync(string symbol, int? limit = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "symbol", symbol }
            };
            parameters.AddOptionalParameter("limit", limit);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/trades", DigifinexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<DigifinexTradesResponse>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get klines/candlestick data
        /// </summary>
        public async Task<WebCallResult<DigifinexKlineResponse>> GetKlinesAsync(
            string symbol,
            DigifinexKlineInterval period,
            DateTime? startTime = null,
            DateTime? endTime = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "symbol", symbol },
                { "period", GetKlineIntervalString(period) }
            };
            parameters.AddOptionalParameter("start_time", DateTimeConverter.ConvertToSeconds(startTime));
            parameters.AddOptionalParameter("end_time", DateTimeConverter.ConvertToSeconds(endTime));

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/kline", DigifinexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<DigifinexKlineResponse>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get spot trading pair symbols
        /// </summary>
        public async Task<WebCallResult<DigifinexSpotSymbolsResponse>> GetSpotSymbolsAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/spot/symbols", DigifinexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<DigifinexSpotSymbolsResponse>(request, null, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get currencies which support margin trading
        /// </summary>
        public async Task<WebCallResult<DigifinexMarginCurrenciesResponse>> GetMarginCurrenciesAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/margin/currencies", DigifinexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<DigifinexMarginCurrenciesResponse>(request, null, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get margin trading pair symbols
        /// </summary>
        public async Task<WebCallResult<DigifinexMarginSymbolsResponse>> GetMarginSymbolsAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/margin/symbols", DigifinexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<DigifinexMarginSymbolsResponse>(request, null, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get whether API trading is enabled for trading pairs
        /// </summary>
        public async Task<WebCallResult<DigifinexTradesSymbolsResponse>> GetTradesSymbolsAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/trades/symbols", DigifinexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<DigifinexTradesSymbolsResponse>(request, null, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get currency deposit and withdrawal information
        /// </summary>
        public async Task<WebCallResult<DigifinexCurrenciesResponse>> GetCurrenciesAsync(string? currency = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currency", currency);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/currencies", DigifinexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<DigifinexCurrenciesResponse>(request, parameters, ct).ConfigureAwait(false);
        }

        private string GetKlineIntervalString(DigifinexKlineInterval interval)
        {
            return interval switch
            {
                DigifinexKlineInterval.OneMinute => "1",
                DigifinexKlineInterval.FiveMinutes => "5",
                DigifinexKlineInterval.FifteenMinutes => "15",
                DigifinexKlineInterval.ThirtyMinutes => "30",
                DigifinexKlineInterval.OneHour => "60",
                DigifinexKlineInterval.FourHours => "240",
                DigifinexKlineInterval.TwelveHours => "720",
                DigifinexKlineInterval.OneDay => "1D",
                DigifinexKlineInterval.OneWeek => "1W",
                _ => throw new ArgumentException($"Unknown kline interval: {interval}")
            };
        }
    }
}