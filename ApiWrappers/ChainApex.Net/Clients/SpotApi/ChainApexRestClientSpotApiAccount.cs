using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using ChainApex.Net.Objects.Models.Account;
using CryptoExchange.Net.Clients;

namespace ChainApex.Net.Clients.SpotApi
{
    public class ChainApexRestClientSpotApiAccount
    {
        private readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        private readonly ChainApexRestClientSpotApi _baseClient;

        internal ChainApexRestClientSpotApiAccount(ChainApexRestClientSpotApi baseClient)
        {
            _baseClient = baseClient;
        }

        public void SetApiCredentials(ApiCredentials credentials)
        {
            _baseClient.SetApiCredentials(credentials);
        }

        public async Task<WebCallResult<ChainApexNewOrderResponse>> PlaceOrderAsync(
            string symbol,
            string side,
            string type,
            decimal volume,
            decimal? price = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("symbol", symbol);
            parameters.Add("side", side);
            parameters.Add("type", type);
            parameters.Add("volume", volume.ToString());
            parameters.AddOptionalParameter("price", price?.ToString());

            var request = _definitions.GetOrCreate(HttpMethod.Post, "/sapi/v1/order", ChainApexExchange.RateLimiter.WeightLimit, 1, true);
            return await _baseClient.SendAsync<ChainApexNewOrderResponse>(request, parameters, ct).ConfigureAwait(false);
        }

        public async Task<WebCallResult<ChainApexNewOrderResponse>> CancelOrderAsync(
            string symbol,
            string orderId,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("symbol", symbol);
            parameters.Add("orderId", orderId);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "/sapi/v1/cancel", ChainApexExchange.RateLimiter.WeightLimit, 1, true);
            return await _baseClient.SendAsync<ChainApexNewOrderResponse>(request, parameters, ct).ConfigureAwait(false);
        }

        public async Task<WebCallResult<ChainApexNewOrderResponse>> GetOrderAsync(
            string symbol,
            string orderId,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("symbol", symbol);
            parameters.Add("orderId", orderId);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/order", ChainApexExchange.RateLimiter.WeightLimit, 1, true);
            return await _baseClient.SendAsync<ChainApexNewOrderResponse>(request, parameters, ct).ConfigureAwait(false);
        }

        public async Task<WebCallResult<List<ChainApexNewOrderResponse>>> GetOpenOrdersAsync(
            string symbol,
            int limit = 1000,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("symbol", symbol);
            parameters.Add("limit", limit);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/openOrders", ChainApexExchange.RateLimiter.WeightLimit, 1, true);
            return await _baseClient.SendAsync<List<ChainApexNewOrderResponse>>(request, parameters, ct).ConfigureAwait(false);
        }

        public async Task<WebCallResult<List<ChainApexMyTrade>>> GetMyTradesAsync(
            string symbol,
            int limit = 100,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("symbol", symbol);
            parameters.Add("limit", limit);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/myTrades", ChainApexExchange.RateLimiter.WeightLimit, 1, true);
            return await _baseClient.SendAsync<List<ChainApexMyTrade>>(request, parameters, ct).ConfigureAwait(false);
        }

        public async Task<WebCallResult<ChainApexAccountInfo>> GetAccountInfoAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/account", ChainApexExchange.RateLimiter.WeightLimit, 1, true);
            return await _baseClient.SendAsync<ChainApexAccountInfo>(request, null, ct).ConfigureAwait(false);
        }
    }
}
