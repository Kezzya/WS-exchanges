using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using Fameex.Net.Objects.Models.ExchangeData;
using FameEX.Net.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FameEX.Net.Clients.SpotApi
{
    public class FameexRestClientSpotApiExchangeData
    {
        private readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        private readonly FameexRestClientSpotApi _baseClient;
        private readonly ILogger _logger;

        internal FameexRestClientSpotApiExchangeData(ILogger logger, FameexRestClientSpotApi baseClient)
        {
            _logger = logger;
            _baseClient = baseClient;
        }

        /// <summary>
        /// Test connectivity to the Rest API
        /// </summary>
        public async Task<WebCallResult<FameexPing>> PingAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/ping", FameexExchange.RateLimiter.WeightLimit, 1);
            var result = await _baseClient.SendAsync<FameexPing>(request, null, ct).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Get server time
        /// </summary>
        public async Task<WebCallResult<Models.FameexServerTime>> GetServerTimeAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/time", FameexExchange.RateLimiter.WeightLimit, 1);
            var result = await _baseClient.SendAsync<Models.FameexServerTime>(request, null, ct).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Get all trading symbols/markets
        /// </summary>
        public async Task<WebCallResult<Models.FameexSymbolsResponse>> GetMarketsAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/symbols", FameexExchange.RateLimiter.WeightLimit, 1);
            var result = await _baseClient.SendAsync<Models.FameexSymbolsResponse>(request, null, ct).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Get all symbols with trading rules
        /// </summary>
        public async Task<WebCallResult<Models.FameexSymbolsResponse>> GetSymbolsAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/symbols", FameexExchange.RateLimiter.WeightLimit, 1);
            var result = await _baseClient.SendAsync<Models.FameexSymbolsResponse>(request, null, ct).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Get ticker for a specific symbol
        /// </summary>
        public async Task<WebCallResult<FameexTickerResponse>> GetTickerAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection { { "symbol", symbol.ToUpper() } };
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/ticker", FameexExchange.RateLimiter.WeightLimit, 1);
            var result = await _baseClient.SendAsync<FameexTickerResponse>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Get klines/candlestick data
        /// </summary>
        public async Task<WebCallResult<IEnumerable<Models.FameexKline>>> GetKlinesAsync(
            string symbol,
            FameexKlineInterval interval,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int? limit = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "symbol", symbol.ToUpper() },
                { "interval", GetKlineIntervalString(interval) }
            };

            if (startTime.HasValue)
                parameters.Add("startTime", new DateTimeOffset(startTime.Value).ToUnixTimeMilliseconds());
            if (endTime.HasValue)
                parameters.Add("endTime", new DateTimeOffset(endTime.Value).ToUnixTimeMilliseconds());
            if (limit.HasValue)
                parameters.Add("limit", limit.Value);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/klines", FameexExchange.RateLimiter.WeightLimit, 1);
            var result = await _baseClient.SendAsync<IEnumerable<Models.FameexKline>>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Get order book depth
        /// </summary>
        public async Task<WebCallResult<FameexOrderBook>> GetOrderBookAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection { { "symbol", symbol.ToUpper() } };
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/depth", FameexExchange.RateLimiter.WeightLimit, 1);
            var result = await _baseClient.SendAsync<FameexOrderBook>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Get recent trades
        /// </summary>
        public async Task<WebCallResult<IEnumerable<FameexTrade>>> GetRecentTradesAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection { { "symbol", symbol.ToUpper() } };
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/trades", FameexExchange.RateLimiter.WeightLimit, 1);
            var result = await _baseClient.SendAsync<IEnumerable<FameexTrade>>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        private string GetKlineIntervalString(FameexKlineInterval interval)
        {
            return interval switch
            {
                FameexKlineInterval.OneMinute => "1m",
                FameexKlineInterval.FiveMinutes => "5m",
                FameexKlineInterval.FifteenMinutes => "15m",
                FameexKlineInterval.ThirtyMinutes => "30m",
                FameexKlineInterval.OneHour => "1h",
                FameexKlineInterval.FourHours => "4h",
                FameexKlineInterval.TwelveHours => "12h",
                FameexKlineInterval.OneDay => "1d",
                FameexKlineInterval.OneWeek => "1w",
                _ => throw new ArgumentException($"Unknown kline interval: {interval}")
            };
        }
    }
}