using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using ChainApex.Net.Objects.Models.ExchangeData;
using CryptoExchange.Net.Clients;

namespace ChainApex.Net.Clients.SpotApi
{
    public class ChainApexRestClientSpotApiExchangeData
    {
        private readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        private readonly ChainApexRestClientSpotApi _baseClient;

        internal ChainApexRestClientSpotApiExchangeData(ChainApexRestClientSpotApi baseClient)
        {
            _baseClient = baseClient;
        }

        public void SetApiCredentials(ApiCredentials credentials)
        {
            _baseClient.SetApiCredentials(credentials);
        }

        public async Task<WebCallResult<ChainApexServerTime>> GetServerTimeAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/time", ChainApexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<ChainApexServerTime>(request, null, ct).ConfigureAwait(false);
        }

        public async Task<WebCallResult<ChainApexOrderBook>> GetOrderBookAsync(string symbol, int? limit = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("symbol", symbol);
            parameters.AddOptionalParameter("limit", limit);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/depth", ChainApexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<ChainApexOrderBook>(request, parameters, ct).ConfigureAwait(false);
        }

        public async Task<WebCallResult<ChainApexTicker>> GetTickerAsync(string symbol, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("symbol", symbol);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/ticker", ChainApexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<ChainApexTicker>(request, parameters, ct).ConfigureAwait(false);
        }

        public async Task<WebCallResult<List<ChainApexKline>>> GetKlinesAsync(
            string symbol, 
            ChainApexKlineInterval interval, 
            DateTime? startTime = null, 
            DateTime? endTime = null, 
            int? limit = null, 
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("symbol", symbol);
            parameters.Add("interval", GetIntervalString(interval));
            parameters.AddOptionalParameter("startTime", startTime != null ? ((DateTimeOffset)startTime.Value).ToUnixTimeMilliseconds() : null);
            parameters.AddOptionalParameter("endTime", endTime != null ? ((DateTimeOffset)endTime.Value).ToUnixTimeMilliseconds() : null);
            parameters.AddOptionalParameter("limit", limit);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/klines", ChainApexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<List<ChainApexKline>>(request, parameters, ct).ConfigureAwait(false);
        }

        public async Task<WebCallResult<ChainApexSymbolsResponse>> GetSymbolsAsync(CancellationToken ct = default)
        {
            // DEBUG: Print method entry and request details
            Console.WriteLine($"[DEBUG] ChainApexRestClientSpotApiExchangeData.GetSymbolsAsync - Method called");
            Console.WriteLine($"[DEBUG] ChainApexRestClientSpotApiExchangeData.GetSymbolsAsync - Request path: /sapi/v1/symbols");
            Console.WriteLine($"[DEBUG] ChainApexRestClientSpotApiExchangeData.GetSymbolsAsync - Request method: GET");
            
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/symbols", ChainApexExchange.RateLimiter.WeightLimit, 1);
            Console.WriteLine($"[DEBUG] ChainApexRestClientSpotApiExchangeData.GetSymbolsAsync - About to call _baseClient.SendAsync");
            
            return await _baseClient.SendAsync<ChainApexSymbolsResponse>(request, null, ct).ConfigureAwait(false);
        }

        public async Task<WebCallResult<List<ChainApexTrade>>> GetRecentTradesAsync(string symbol, int limit = 100, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("symbol", symbol);
            parameters.Add("limit", limit);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/trades", ChainApexExchange.RateLimiter.WeightLimit, 1);
            return await _baseClient.SendAsync<List<ChainApexTrade>>(request, parameters, ct).ConfigureAwait(false);
        }

        private string GetIntervalString(ChainApexKlineInterval interval)
        {
            return interval switch
            {
                ChainApexKlineInterval.OneMinute => "1min",
                ChainApexKlineInterval.FiveMinutes => "5min",
                ChainApexKlineInterval.FifteenMinutes => "15min",
                ChainApexKlineInterval.ThirtyMinutes => "30min",
                ChainApexKlineInterval.OneHour => "60min",
                ChainApexKlineInterval.FourHours => "4h",
                ChainApexKlineInterval.OneDay => "1day",
                ChainApexKlineInterval.OneWeek => "1week",
                _ => "1min"
            };
        }
    }
}
