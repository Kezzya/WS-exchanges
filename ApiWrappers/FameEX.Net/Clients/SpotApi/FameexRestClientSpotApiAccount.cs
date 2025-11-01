using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using FameEX.Net.Models;
using FameEX.Net.Objects.Models.Account;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FameexOrderStatus = FameEX.Net.Objects.Models.Account.FameexOrderStatus;

namespace FameEX.Net.Clients.SpotApi
{
    public class FameexRestClientSpotApiAccount
    {
        private readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        private readonly FameexRestClientSpotApi _baseClient;
        private readonly FameexTradingRulesTracker _tradingRulesTracker;
        private FameexSocketClientSpotApi _socketClient;
        private readonly HashSet<string> _subscribedSymbols = new HashSet<string>();
        private readonly ILogger _logger;

        internal FameexRestClientSpotApiAccount(ILogger logger, FameexRestClientSpotApi baseClient)
        {
            _logger = logger;
            _baseClient = baseClient;
            _tradingRulesTracker = new FameexTradingRulesTracker();
        }

        public void AddSocket(FameexSocketClientSpotApi socketClient)
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

            var result = await _socketClient.SubscribeToOrderUpdatesAsync(
                newSymbols,
                update =>
                {
                    var order = update.Data;
                   
                            _tradingRulesTracker.RecordOrderUpdate(
                                order.Symbol,
                                order.OrderId,
                                order.FilledAmount);
                        
                    
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
        /// Get account information with balances
        /// </summary>
        public async Task<WebCallResult<FameexAccountInfo>> GetAccountInfoAsync(CancellationToken ct = default)
        {
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/sapi/v1/account", FameexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<FameexAccountInfo>(request, null, ct).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Get account balances only
        /// </summary>
        public async Task<WebCallResult<Dictionary<string, decimal>>> GetBalancesAsync(
            string? asset = null,
            CancellationToken ct = default)
        {
            var accountInfo = await GetAccountInfoAsync(ct).ConfigureAwait(false);

            if (!accountInfo.Success || accountInfo.Data?.Balances == null)
                return accountInfo.AsError<Dictionary<string, decimal>>(accountInfo.Error!);

            var balances = accountInfo.Data.Balances
                .Where(b => asset == null || b.Asset.Equals(asset, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(
                    b => b.Asset,
                    b =>
                    {
                        decimal.TryParse(b.Free, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var free);
                        decimal.TryParse(b.Locked, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var locked);
                        return free + locked;
                    }
                );

            return accountInfo.As(balances);
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        public async Task<WebCallResult<Objects.Models.Account.FameexOrder>> GetOrderAsync(
            string symbol,
            string orderId,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "symbol", symbol },
                { "order_id", orderId }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v1/order", FameexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<FameexResponse<Objects.Models.Account.FameexOrder>>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data?.Data);
        }

        /// <summary>
        /// Get order history
        /// </summary>
        public async Task<WebCallResult<List<Objects.Models.Account.FameexOrder>>> GetOrderHistoryAsync(
            string symbol,
            FameexOrderStatus? status = null,
            int? page = null,
            int? limit = null,
            long? startTime = null,
            long? endTime = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "symbol", symbol }
            };
            parameters.AddOptionalParameter("status", status?.ToString().ToLowerInvariant());
            parameters.AddOptionalParameter("page", page);
            parameters.AddOptionalParameter("limit", limit);
            parameters.AddOptionalParameter("start_time", startTime);
            parameters.AddOptionalParameter("end_time", endTime);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v1/orders", FameexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<FameexResponse<List<Objects.Models.Account.FameexOrder>>>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data?.Data);
        }

        /// <summary>
        /// Get open orders
        /// </summary>
        public async Task<WebCallResult<List<Objects.Models.Account.FameexOrder>>> GetOpenOrdersAsync(
            string? symbol = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("symbol", symbol);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v1/orders/open", FameexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<FameexResponse<List<Objects.Models.Account.FameexOrder>>>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data?.Data);
        }

        /// <summary>
        /// Get trade history
        /// </summary>
        public async Task<WebCallResult<List<FameexUserTrade>>> GetTradeHistoryAsync(
            string symbol,
            string? orderId = null,
            int? page = null,
            int? limit = null,
            long? startTime = null,
            long? endTime = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "symbol", symbol }
            };
            parameters.AddOptionalParameter("order_id", orderId);
            parameters.AddOptionalParameter("page", page);
            parameters.AddOptionalParameter("limit", limit);
            parameters.AddOptionalParameter("start_time", startTime);
            parameters.AddOptionalParameter("end_time", endTime);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v1/trades", FameexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<FameexResponse<List<FameexUserTrade>>>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data?.Data);
        }

        /// <summary>
        /// Place new order with trading rules check
        /// </summary>
        public async Task<WebCallResult<FameexNewOrderResponse>> PlaceOrderAsync(
            string symbol,
            FameexOrderType type,
            decimal quantity,
            decimal? price = null,
            string? clientOrderId = null,
            CancellationToken ct = default)
        {
            var (isAllowed, violationReason) = _tradingRulesTracker.CheckTradingRules(symbol);
            if (!isAllowed)
            {
                return new WebCallResult<FameexNewOrderResponse>(
                    new ServerRateLimitError($"Trading rules violation: {violationReason}"));
            }

            var parameters = new ParameterCollection
            {
                { "symbol", symbol },
                { "type", type.ToString().ToLowerInvariant() },
                { "amount", quantity.ToString(System.Globalization.CultureInfo.InvariantCulture) }
            };
            parameters.AddOptionalParameter("price", price?.ToString(System.Globalization.CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("client_order_id", clientOrderId);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "sapi/v1/order", FameexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<FameexNewOrderResponse>(request, parameters, ct).ConfigureAwait(false);

            if (result is { Success: true, Data.OrderId: not null })
            {
                _tradingRulesTracker.RecordOrderPlaced(symbol, result.Data.OrderId, quantity);

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
        /// Place batch orders with trading rules check
        /// </summary>
        public async Task<WebCallResult<FameexBatchOrderResponse>> PlaceBatchOrdersAsync(
            string symbol,
            List<FameexBatchOrderRequest> orders,
            CancellationToken ct = default)
        {
            var (isAllowed, violationReason) = _tradingRulesTracker.CheckTradingRules(symbol);
            if (!isAllowed)
            {
                return new WebCallResult<FameexBatchOrderResponse>(
                    new ServerRateLimitError($"Trading rules violation: {violationReason}"));
            }

            var parameters = new ParameterCollection
            {
                { "symbol", symbol },
                { "orders", JsonConvert.SerializeObject(orders) }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, "/v1/order/batch", FameexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<FameexBatchOrderResponse>(request, parameters, ct).ConfigureAwait(false);

            if (result is not { Success: true, Data.OrderIds: not null })
            {
                return result;
            }

            foreach (var orderId in result.Data.OrderIds.Where(o => !string.IsNullOrEmpty(o)))
            {
                _tradingRulesTracker.RecordOrderPlaced(symbol, orderId, 0); // Amount from batch request
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
        public async Task<WebCallResult<FameexCancelOrderResponse>> CancelOrderAsync(
            string symbol,
            string orderId,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "symbol", symbol },
                { "order_id", orderId }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, "/v1/order/cancel", FameexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<FameexCancelOrderResponse>(request, parameters, ct).ConfigureAwait(false);

            if (result.Success && result.Data?.Success?.Contains(orderId) == true)
            {
                _tradingRulesTracker.RecordOrderCancelled(symbol, orderId);
            }

            return result.As(result.Data);
        }

        /// <summary>
        /// Cancel all orders for a symbol
        /// </summary>
        public async Task<WebCallResult<FameexCancelOrderResponse>> CancelAllOrdersAsync(
            string symbol,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "symbol", symbol }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, "/v1/order/cancel_all", FameexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<FameexCancelOrderResponse>(request, parameters, ct).ConfigureAwait(false);

            if (result.Success && result.Data?.Success != null)
            {
                foreach (var orderId in result.Data.Success)
                {
                    _tradingRulesTracker.RecordOrderCancelled(symbol, orderId);
                }
            }

            return result.As(result.Data);
        }

        /// <summary>
        /// Get deposit address
        /// </summary>
        public async Task<WebCallResult<List<FameexDepositAddress>>> GetDepositAddressAsync(
            string currency,
            string? network = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection
            {
                { "currency", currency }
            };
            parameters.AddOptionalParameter("network", network);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v1/deposit/address", FameexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<FameexResponse<List<FameexDepositAddress>>>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data?.Data);
        }

        /// <summary>
        /// Get deposit history
        /// </summary>
        public async Task<WebCallResult<List<FameexDepositHistory>>> GetDepositHistoryAsync(
            string? currency = null,
            int? page = null,
            int? limit = null,
            long? startTime = null,
            long? endTime = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currency", currency);
            parameters.AddOptionalParameter("page", page);
            parameters.AddOptionalParameter("limit", limit);
            parameters.AddOptionalParameter("start_time", startTime);
            parameters.AddOptionalParameter("end_time", endTime);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v1/deposit/history", FameexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<FameexResponse<List<FameexDepositHistory>>>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data?.Data);
        }

        /// <summary>
        /// Get withdrawal history
        /// </summary>
        public async Task<WebCallResult<List<FameexWithdrawHistory>>> GetWithdrawHistoryAsync(
            string? currency = null,
            int? page = null,
            int? limit = null,
            long? startTime = null,
            long? endTime = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalParameter("currency", currency);
            parameters.AddOptionalParameter("page", page);
            parameters.AddOptionalParameter("limit", limit);
            parameters.AddOptionalParameter("start_time", startTime);
            parameters.AddOptionalParameter("end_time", endTime);

            var request = _definitions.GetOrCreate(HttpMethod.Get, "/v1/withdraw/history", FameexExchange.RateLimiter.WeightLimit, 1, true);
            var result = await _baseClient.SendAsync<FameexResponse<List<FameexWithdrawHistory>>>(request, parameters, ct).ConfigureAwait(false);
            return result.As(result.Data?.Data);
        }


        #endregion
    }
}