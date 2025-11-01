using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using Digifinex.Net.Objects.Models.Account;
using Newtonsoft.Json;
using DigifinexOrderStatus = Digifinex.Net.Objects.Models.Socket.DigifinexOrderStatus;

namespace Digifinex.Net.Clients.SpotApi
{
    public class DigifinexRestClientSpotApiAccount
    {
        private readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        private readonly DigifinexRestClientSpotApi _baseClient;
        private readonly DigifinexTradingRulesTracker _tradingRulesTracker;
        private DigifinexSocketClientSpotApi _socketClient;
        private readonly HashSet<string> _subscribedSymbols = new HashSet<string>();

        internal DigifinexRestClientSpotApiAccount(DigifinexRestClientSpotApi baseClient)
        {
            _baseClient = baseClient;
            _tradingRulesTracker = new DigifinexTradingRulesTracker();
        }

        public void AddSocket(DigifinexSocketClientSpotApi socketClient)
        {
            _socketClient = socketClient;
        }

        /// <summary>
        /// Subscribe to order updates for tracking fills
        /// </summary>
        private async Task TrySubscribeTrackerToOrderUpdatesAsync(
            List<string> symbols,
            CancellationToken ct = default)
        {
            if (_socketClient == null)
            {
                return;
            }

            var newSymbols = symbols.Where(s => !_subscribedSymbols.Contains(s)).ToList();
            if (!newSymbols.Any())
            {
                return;
            }

            var result = await _socketClient.SubscribeToOrdersAsync(
                newSymbols,
                update =>
                {
                    foreach (var order in update.Data)
                    {
                        if (string.IsNullOrEmpty(order.Symbol) || string.IsNullOrEmpty(order.Id))
                            continue;

                        if (order.Status is DigifinexOrderStatus.PartiallyFilled or DigifinexOrderStatus.FullyFilled)
                        {
                            _tradingRulesTracker.RecordOrderUpdate(
                                order.Symbol, 
                                order.Id, 
                                order.Filled);
                        }
                    }
                },
                ct);

            if (result.Success)
            {
                foreach (var symbol in newSymbols)
                {
                    _subscribedSymbols.Add(symbol);
                }
            }
        }

        

        #region Account Endpoints

        /// <summary>
        /// Get spot account assets
        /// </summary>
        public async Task<WebCallResult<List<DigifinexAsset>>> GetSpotAssetsAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/spot/assets", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<DigifinexResponse<List<DigifinexAsset>>>(request, null, ct).ConfigureAwait(false);
            return result.As(result.Data?.List);
        }

        /// <summary>
        /// Get margin assets
        /// </summary>
        public async Task<WebCallResult<DigifinexMarginAssets>> GetMarginAssetsAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/margin/assets", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<DigifinexResponse<DigifinexMarginAssets>>(request, null, ct).ConfigureAwait(false);
            
            return result.As(result.Data?.Data);
        }

        /// <summary>
        /// Get financial logs
        /// </summary>
        public async Task<WebCallResult<DigifinexFinanceLogResponse>> GetFinanceLogAsync(
            DigifinexMarketType market,
            string? currencyMark = null,
            long? startTime = null,
            long? endTime = null,
            int? limit = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currency_mark", currencyMark);
            parameters.AddOptionalParameter("start_time", startTime);
            parameters.AddOptionalParameter("end_time", endTime);
            parameters.AddOptionalParameter("limit", limit);

            var marketStr = market.ToString().ToLowerInvariant();
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"/v3/{marketStr}/financelog", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<DigifinexResponse<DigifinexFinanceLogResponse>>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data?.Data);
        }

        /// <summary>
        /// Get order status
        /// </summary>
        public async Task<WebCallResult<List<DigifinexOrder>>> GetOrderAsync(
            DigifinexMarketType market,
            string orderId,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "order_id", orderId }
            };

            var marketStr = market.ToString().ToLowerInvariant();
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"/v3/{marketStr}/order", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<DigifinexResponse<List<DigifinexOrder>>>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data?.Data);
        }

        /// <summary>
        /// Get order detail
        /// </summary>
        public async Task<WebCallResult<DigifinexOrderDetail>> GetOrderDetailAsync(
            DigifinexMarketType market,
            string orderId,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "order_id", orderId }
            };

            var marketStr = market.ToString().ToLowerInvariant();
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"/v3/{marketStr}/order/detail", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<DigifinexResponse<DigifinexOrderDetail>>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data?.Data);
        }

        /// <summary>
        /// Get current active orders
        /// </summary>
        public async Task<WebCallResult<List<DigifinexOrder>>> GetCurrentOrdersAsync(
            DigifinexMarketType market,
            string? symbol = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("symbol", symbol);

            var marketStr = market.ToString().ToLowerInvariant();
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"/v3/{marketStr}/order/current", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<DigifinexResponse<List<DigifinexOrder>>>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data?.Data);
        }

        /// <summary>
        /// Get order history
        /// </summary>
        public async Task<WebCallResult<List<DigifinexOrder>>> GetOrderHistoryAsync(
            DigifinexMarketType market,
            string? symbol = null,
            int? limit = null,
            long? startTime = null,
            long? endTime = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("symbol", symbol);
            parameters.AddOptionalParameter("limit", limit);
            parameters.AddOptionalParameter("start_time", startTime);
            parameters.AddOptionalParameter("end_time", endTime);

            var marketStr = market.ToString().ToLowerInvariant();
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"/v3/{marketStr}/order/history", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<DigifinexResponse<List<DigifinexOrder>>>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data?.Data);
        }

        /// <summary>
        /// Get my trades
        /// </summary>
        public async Task<WebCallResult<List<DigifinexTrade>>> GetMyTradesAsync(
            DigifinexMarketType market,
            string? symbol = null,
            int? limit = null,
            long? startTime = null,
            long? endTime = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("symbol", symbol);
            parameters.AddOptionalParameter("limit", limit);
            parameters.AddOptionalParameter("start_time", startTime);
            parameters.AddOptionalParameter("end_time", endTime);

            var marketStr = market.ToString().ToLowerInvariant();
            var request = _definitions.GetOrCreate(HttpMethod.Get, $"/v3/{marketStr}/mytrades", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<DigifinexResponse<List<DigifinexTrade>>>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data?.List);
        }

        /// <summary>
        /// Get margin positions
        /// </summary>
        public async Task<WebCallResult<DigifinexMarginPositions>> GetMarginPositionsAsync(
            string? symbol = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("symbol", symbol);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/margin/positions", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<DigifinexMarginPositions>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data);
        }

        /// <summary>
        /// Create new order with trading rules check
        /// </summary>
        public async Task<WebCallResult<DigifinexNewOrderResponse>> PlaceOrderAsync(
            DigifinexMarketType market,
            string symbol,
            string type,
            decimal amount,
            decimal? price = null,
            int? postOnly = null,
            CancellationToken ct = default)
        {
            var (isAllowed, violationReason) = _tradingRulesTracker.CheckTradingRules(symbol);
            if (!isAllowed)
            {
                return new WebCallResult<DigifinexNewOrderResponse>(
                    new ServerRateLimitError($"Trading rules violation: {violationReason}"));
            }

            var parameters = new ParameterCollection
            {
                { "symbol", symbol },
                { "type", type },
                { "amount", amount }
            };
            parameters.AddOptionalParameter("price", price);
            parameters.AddOptionalParameter("post_only", postOnly);

            var marketStr = market.ToString().ToLowerInvariant();
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"/v3/{marketStr}/order/new", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<DigifinexNewOrderResponse>(request, parameters, ct).ConfigureAwait(false);

            if (result is { Success: true, Data.OrderId: not null })
            {
                _tradingRulesTracker.RecordOrderPlaced(symbol, result.Data.OrderId, amount);

                if (_socketClient != null && !_subscribedSymbols.Contains(symbol))
                {
                    _ = Task.Run(async () => 
                    {
                        await TrySubscribeTrackerToOrderUpdatesAsync(new List<string> { symbol }, CancellationToken.None);
                    });
                }
            }

            return result.As(result.Data);
        }

        /// <summary>
        /// Create multiple orders with trading rules check
        /// </summary>
        public async Task<WebCallResult<DigifinexBatchOrderResponse>> PlaceBatchOrdersAsync(
            DigifinexMarketType market,
            string symbol,
            List<DigifinexBatchOrderRequest> orders,
            CancellationToken ct = default)
        {
            var (isAllowed, violationReason) = _tradingRulesTracker.CheckTradingRules(symbol);
            if (!isAllowed)
            {
                return new WebCallResult<DigifinexBatchOrderResponse>(
                    new ServerRateLimitError($"Trading rules violation: {violationReason}"));
            }

            var parameters = new ParameterCollection
            {
                { "symbol", symbol },
                { "list", JsonConvert.SerializeObject(orders) }
            };

            var marketStr = market.ToString().ToLowerInvariant();
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"/v3/{marketStr}/order/batch_new", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<DigifinexBatchOrderResponse>(request, parameters, ct).ConfigureAwait(false);

            if (result is not { Success: true, Data.OrderIds: not null })
            {
                return result;
            }

            foreach (var order in result.Data.OrderIds.Where(o => !string.IsNullOrEmpty(o)))
            {
                _tradingRulesTracker.RecordOrderPlaced(symbol, order, 0); //TODO: Пока так
            }

            if (_socketClient != null && !_subscribedSymbols.Contains(symbol))
            {
                _ = Task.Run(async () => 
                {
                    await TrySubscribeTrackerToOrderUpdatesAsync(new List<string> { symbol }, CancellationToken.None);
                });
            }

            return result.As(result.Data);
        }

        /// <summary>
        /// Cancel order with tracking
        /// </summary>
        public async Task<WebCallResult<DigifinexCancelOrderResponse>> CancelOrderAsync(
            DigifinexMarketType market,
            string symbol,
            string orderId,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "order_id", orderId }
            };

            var marketStr = market.ToString().ToLowerInvariant();
            var request = _definitions.GetOrCreate(HttpMethod.Post, $"/v3/{marketStr}/order/cancel", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<DigifinexCancelOrderResponse>(request, parameters, ct).ConfigureAwait(false);

            if (result.Success)
            {
                _tradingRulesTracker.RecordOrderCancelled(symbol, orderId);
            }

            return result.As(result.Data);
        }

        /// <summary>
        /// Transfer assets among accounts
        /// </summary>
        public async Task<WebCallResult<object>> TransferAsync(
            string currencyMark,
            string num,
            DigifinexAccountType from,
            DigifinexAccountType to,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "currency_mark", currencyMark },
                { "num", num },
                { "from", (int)from },
                { "to", (int)to }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, "/v3/transfer", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            return await _baseClient.SendAsync<object>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Close margin positions
        /// </summary>
        public async Task<WebCallResult<object>> CloseMarginPositionAsync(
            string symbol,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "symbol", symbol }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, "/v3/margin/position/close", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            return await _baseClient.SendAsync<object>(request, parameters, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get deposit address
        /// </summary>
        public async Task<WebCallResult<List<DigifinexDepositAddress>>> GetDepositAddressAsync(
            string currency,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "currency", currency }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/deposit/address", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<DigifinexResponse<List<DigifinexDepositAddress>>>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data?.Data);
        }

        /// <summary>
        /// Get deposit history
        /// </summary>
        public async Task<WebCallResult<List<DigifinexDepositHistory>>> GetDepositHistoryAsync(
            string? currency = null,
            int? from = null,
            int? size = null,
            string? direct = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currency", currency);
            parameters.AddOptionalParameter("from", from);
            parameters.AddOptionalParameter("size", size);
            parameters.AddOptionalParameter("direct", direct);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/deposit/history", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<DigifinexResponse<List<DigifinexDepositHistory>>>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data?.Data);
        }

        /// <summary>
        /// Get withdraw history
        /// </summary>
        public async Task<WebCallResult<List<DigifinexWithdrawHistory>>> GetWithdrawHistoryAsync(
            string? currency = null,
            string? from = null,
            string? size = null,
            string? direct = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currency", currency);
            parameters.AddOptionalParameter("from", from);
            parameters.AddOptionalParameter("size", size);
            parameters.AddOptionalParameter("direct", direct);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v3/withdraw/history", DigifinexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<DigifinexResponse<List<DigifinexWithdrawHistory>>>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data?.Data);
        }

        #endregion
    }
}