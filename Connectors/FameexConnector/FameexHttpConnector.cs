using BaseStockConnector;
using BaseStockConnector.Models.Enums;
using BaseStockConnector.Models.Instruments;
using BaseStockConnector.Models.Orders;
using BaseStockConnectorInterface;
using BaseStockConnectorInterface.Helper;
using BaseStockConnectorInterface.Logger;
using BaseStockConnectorInterface.Models;
using BaseStockConnectorInterface.Models.Enums;
using BaseStockConnectorInterface.Models.History;
using BaseStockConnectorInterface.Models.Instruments;
using BaseStockConnectorInterface.Models.Kline;
using BaseStockConnectorInterface.Models.Orders;
using BaseStockConnectorInterface.Models.Ticker;
using CryptoExchange.Net.RateLimiting;
using FameEX.Net;
using FameEX.Net.Clients;
using FameEX.Net.Clients.SpotApi;
using FameEX.Net.Models;
using FameEX.Net.Objects.Options;
using Microsoft.Extensions.Logging;
using StockConnector;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using AccountType = BaseStockConnectorInterface.Models.Enums.AccountType;
using OrderType = BaseStockConnector.Models.Enums.OrderType;
using WithdrawHistory = BaseStockConnectorInterface.Models.WithdrawHistory;

namespace FameexConnector
{
    internal class FameexHttpConnector : IStockHttp
    {
        private const string StockName = "FameEX";

        public FameexRestClientSpotApi _client;

        private ILogger _logger;
        private readonly FameexSocketConnector _connector;

        public event Action<RateLimitEvent> RateLimitTriggered;

        private ConcurrentDictionary<string, string> _orderInternalIdsDictionary;


        public FameexHttpConnector(FameexSocketConnector connector, ConcurrentDictionary<string, string> orderInternalIdsDictionary)
        {
            _connector = connector;
            _orderInternalIdsDictionary = orderInternalIdsDictionary;
        }

        public async Task ConnectAsync(IStockCredential stockCredential, ILogger clientLogger, List<WebProxy>? proxies, HttpClientHandler? httpClientHandler)
        {
            if (_client != null)
            {
                return;
            }

            FameexCredential credential;
            _logger = clientLogger;

            try
            {
                credential = (FameexCredential)stockCredential;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }

            var logFactory = new LoggerFactory();
            logFactory.AddProvider(new ExchangeLoggerProvider(new PrefixedLogger(_logger, StockName)));

            var httpClient = httpClientHandler != null ? new HttpClient(httpClientHandler) : new HttpClient();
            var options = new FameexRestOptions
            {
                ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials(credential.ApiKey, credential.Secret),
                RateLimiterEnabled = true,
                Environment = FameexEnvironment.Live
            };
            _client = new FameexRestClientSpotApi(_logger, httpClient, options);

            await _connector.ConnectAsync(credential, _logger, null);
            await Task.CompletedTask;
        }

        public async Task ConnectAsync(IStockCredential stockCredential, ILogger clientLogger, WebProxy proxy)
        {
            if (_client != null)
            {
                return;
            }

            FameexCredential credential;
            _logger = clientLogger;

            try
            {
                credential = (FameexCredential)stockCredential;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }

            var logFactory = new LoggerFactory();
            logFactory.AddProvider(new ExchangeLoggerProvider(new PrefixedLogger(_logger, StockName)));

            var options = new FameexRestOptions
            {
                ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials(credential.ApiKey, credential.Secret),
                RateLimiterEnabled = true,
            };
            if (proxy != null)
            {
                var host = $"{proxy.Address!.Scheme}://{proxy.Address!.Host}";
                var port = proxy.Address!.Port;
                options.Proxy = new CryptoExchange.Net.Objects.ApiProxy(host, port);
            }
            _client = new FameexRestClientSpotApi(_logger, null, options);

            await _connector.ConnectAsync(credential, _logger, null);
            await Task.CompletedTask;
        }
        #region PublicApi

        public async Task<DateTime> GetServerTimestamp()
        {
            var timestamp = await _client.ExchangeData.GetServerTimeAsync();

            if (timestamp.Success)
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(timestamp.Data.ServerTime).UtcDateTime;
            }

            return DateTime.UtcNow;
        }

        public async Task<SpotCoinModel> GetCoinInfoAsync(string instrumentName, InstrumentType instrumentType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }
            if (instrumentType != InstrumentType.Spot)
            {
                throw new ArgumentException($"Exchange: {StockName}, Unsupported instrumentType: {instrumentType}", nameof(instrumentType));
            }

            var data = await _client.ExchangeData.GetSymbolsAsync(); // Исправлено
            if (!data.Success)
            {
                return new SpotCoinModel { InstrumentName = instrumentName };
            }

            var instrument = data.Data.Symbols.FirstOrDefault(x => string.Equals(x.Symbol, instrumentName, StringComparison.OrdinalIgnoreCase));
            if (instrument == null)
            {
                return new SpotCoinModel { InstrumentName = instrumentName };
            }

        
            var baseSymbol = instrument.BaseAsset;
            var quoteSymbol = instrument.QuoteAsset;

            return new SpotCoinModel
            {
                InstrumentName = instrumentName,
                InstrumentType = InstrumentType.Spot,
                StockName = StockName,
                OriginalInstrumentName = instrumentName,
                MinMovementVolume = MinMovement(instrument.QuantityPrecision),
                MinMovement = MinMovement(instrument.PricePrecision),
                MinVolumeInBaseCurrency = instrument.LimitVolumeMin,
                BaseCurrency = baseSymbol,
                QuoteCurrency = quoteSymbol
            };
        }
        public Task<ExchangeResponse<List<OptionInfo>>> GetOptionList(string? instrumentName, DateTime? expirationDate)
        {
            throw new NotImplementedException();
        }

        public Task<ExchangeResponse<List<Volatility>>> GetVolatility(string instrumentName, DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        private decimal MinMovement(int precision)
        {
            if (precision == 0)
            {
                return 0;
            }

            var minMovement = 0.1m;
            for (var i = 1; i < precision; i++)
            {
                minMovement *= 0.1m;
            }
            return minMovement;
        }

        public async Task<List<SpotCoinModel>> GetCoinInfoAsync(InstrumentType instrumentType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            if (instrumentType != InstrumentType.Spot)
            {
                throw new ArgumentException($"Exchange: {StockName}, Unsupported instrumentType: {instrumentType}", nameof(instrumentType));
            }

            var data = await _client.GetSymbolsAsync();
            if (!data.Success)
            {
                throw new Exception(data.Error?.ToString());
            }

            return data.Data.Select(x => new SpotCoinModel
            {
                InstrumentName = x.Name,
                InstrumentType = InstrumentType.Spot,
                StockName = StockName,
                OriginalInstrumentName = x.Name,
                MinMovementVolume = x.QuantityStep ?? 0.00000001m,
                MinMovement = x.PriceStep ?? 0.00000001m,
                MinVolumeInBaseCurrency = x.MinTradeQuantity ?? 0,
            }).ToList();
        }

        public async Task<decimal> Get24hVolume(string coinName, InstrumentType instrumentType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var ticker = await _client.ExchangeData.GetTickerAsync(coinName);
            return ticker is { Success: true, Data: not null }
                ? ticker.Data.Volume
                : 0m;
        }

        public async Task<decimal> Get24hVolume(string coinName, DateTime date, InstrumentType instrumentType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var start = date.Date;
            var end = date.AddDays(1) < DateTime.UtcNow ? date.AddDays(1) : DateTime.UtcNow;

            var kLineInfo = await _client.GetKlinesAsync(coinName, TimeSpan.FromMinutes(5), limit: 288);
            if (kLineInfo is { Success: true, Data: not null })
            {
                return kLineInfo.Data
                    .Where(k => k.Volume.HasValue)
                    .Sum(k => k.Volume.Value); // Volume is at index 5
            }
            return 0m;
        }

        public async Task<OrderBook> GetDepthAsync(InstrumentType instrumentType, string symbol, int depth)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            if (instrumentType != InstrumentType.Spot)
            {
                return null;
            }

            //var data = await _client.GetOrderBookAsync(symbol, depth);

            //var result = data.Data;
            //if (data.Success && result != null)
            //{
            //    return new OrderBook
            //    {
            //        InstrumentName = symbol,
            //        InstrumentType = instrumentType,
            //        OriginalInstrumentName = symbol,
            //        StockName = StockName,
            //        SystemTime = DateTime.UtcNow,
            //        Asks = result.Asks
            //            .Where(x => x.Length >= 2)
            //            .OrderBy(x => Convert.ToDecimal(x[0]))
            //            .Select(x => new OrderBookEntry { Price = Convert.ToDecimal(x[0]), Amount = Convert.ToDecimal(x[1]) })
            //            .ToList(),
            //        Bids = result.Bids
            //            .Where(x => x.Length >= 2)
            //            .OrderByDescending(x => Convert.ToDecimal(x[0]))
            //            .Select(x => new OrderBookEntry { Price = Convert.ToDecimal(x[0]), Amount = Convert.ToDecimal(x[1]) })
            //            .ToList(),
            //    };
            //}

            return null;
        }

        public async Task<List<KlineItem>> GetKlines(string instrumentName, KlineInterval klineInterval, DateTime? fromDateUtc, DateTime? toDateUtc, int? limit = null, InstrumentType instrumentType = InstrumentType.Spot)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var interval = FameexExtensions.ToFameexKlineInterval(klineInterval); // Явно указываем класс
            var timespan = FameexExtensions.ToTimeSpan(interval);

            var data = await _client.GetKlinesAsync(
                instrumentName,
                timespan,
                fromDateUtc,
                toDateUtc,
                limit);

            if (data is { Success: true, Data: not null })
            {
                return data.Data.Select(x => new KlineItem
                {
                    OpenTime = x.OpenTime,
                    OpenPrice = (decimal)x.OpenPrice,
                    HighPrice = (decimal)x.HighPrice,
                    LowPrice = (decimal)x.LowPrice,
                    ClosePrice = (decimal)x.ClosePrice,
                    Volume = x.Volume ?? 0,
                }).ToList();
            }

            return new List<KlineItem>();
        }

        public async Task<OrderInfo> GetLastTrade(string instrumentName, InstrumentType instrumentType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var trades = await _client.GetRecentTradesAsync(instrumentName);

            if (!trades.Success || !trades.Data.Any())
            {
                return null;
            }

            var lastTrade = trades.Data.First();
            if (lastTrade == null)
            {
                return null;
            }

            return new OrderInfo
            {
                InstrumentName = instrumentName,
                StockName = StockName,
                InstrumentType = InstrumentType.Spot,
                Price = lastTrade.Price,
                Volume = lastTrade.Quantity,
                OriginalInstrumentName = instrumentName,
                TransactionDate = lastTrade.Timestamp,
                TradeId = lastTrade.Timestamp.ToString()
            };
        }

        public Task<BalanceTransfer> Transfer(AccountType from, AccountType to, string instrumentName, decimal amount)
        {
            throw new NotImplementedException();
        }

        #endregion PublicApi

        #region PrivateApi

        public async Task<List<BaseOrderModel>> GetActiveOrdersAsync(string instrumentName, InstrumentType instrumentType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            if (instrumentType == InstrumentType.Spot)
            {
                // FameEX doesn't have a direct "get current orders" endpoint in the docs
                // You may need to implement this based on their actual API
                throw new NotImplementedException("GetActiveOrdersAsync not available in FameEX API");
            }
            return null;
        }

        public async Task<OrderSyncInfo?> GetOrderInfo(string instrumentName, string orderId, InstrumentType instrumentType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var data = await _client.GetOrderAsync(orderId, instrumentName);
            if (!data.Success || data.Data == null)
            {
                return null;
            }

            var order = data.Data;

            return new OrderSyncInfo
            {
                Price = order.Price ?? 0,
                FilledVolume = order.QuantityFilled ?? 0,
                StockOrderId = orderId,
                Volume = order.Quantity ?? 0,
                InstrumentName = instrumentName,
                Direction = order.Side == CryptoExchange.Net.CommonObjects.CommonOrderSide.Buy ? Direction.BUY : Direction.SELL,
                DateCreated = DateTime.UtcNow,
                DateUpdated = order.Timestamp,
                Status = ConvertOrderStatus(order.Status),
                AvgPrice = (order.QuantityFilled ?? 0) > 0
                    ? (order.Quantity ?? 0) / (order.QuantityFilled ?? 1)
                    : (order.Price ?? 0)
            };
        }

        public async Task<BaseOrderModel> CancelOrderAsync(InstrumentType instrumentType, string instrumentName, string orderId)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            if (instrumentType != InstrumentType.Spot)
            {
                return new BaseOrderModel { Success = false, Error = $"{StockName} only spot orders type allow" };
            }

            if (string.IsNullOrEmpty(orderId))
            {
                return new BaseOrderModel { Success = false, Error = $"{StockName} try cancel order with empty Id!" };
            }

            var cancelOrder = await _client.CancelOrderAsync(orderId, instrumentName.ToLower());
            if (cancelOrder is { Success: true, Data: not null })
            {
                return new BaseOrderModel
                {
                    Success = true,
                    StockOrderId = orderId,
                    StockName = StockName,
                    InstrumentName = instrumentName,
                };
            }

            var errorResult = new BaseOrderModel
            {
                Error = $"{cancelOrder.Error?.Code} {cancelOrder.Error?.Message}",
                Success = false,
                InstrumentName = instrumentName,
                OriginalInstrumentName = instrumentName,
                StockName = StockName,
                InstrumentType = InstrumentType.Spot
            };

            var orderInfoResponse = await GetOrderInfo(instrumentName, orderId, instrumentType);
            if (orderInfoResponse != null)
            {
                if (orderInfoResponse.Status == OrderHistoryItemStatus.Filled)
                {
                    errorResult.ErrorType = BaseOrderModel.OrderErrorType.AlreadyFilled;
                }
                else if (orderInfoResponse.Status == OrderHistoryItemStatus.Canceled)
                {
                    errorResult.ErrorType = BaseOrderModel.OrderErrorType.AlreadyCancelled;
                }
            }
            else
            {
                errorResult.ErrorType = BaseOrderModel.OrderErrorType.Unknown;
            }

            return errorResult;
        }

        public async Task<BaseOrderModel> MakeOrderAsync(InstrumentType instrumentType, OrderType orderType, Direction direction, PositionOrderType positionOrderType, string instrument, decimal volume, decimal? price, string orderId)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var result = new BaseOrderModel
            {
                Success = false,
                StockName = StockName,
                InstrumentType = instrumentType,
                InstrumentName = instrument,
                OriginalInstrumentName = instrument,
                Type = orderType,
                Direction = direction,
            };

            if (instrumentType != InstrumentType.Spot)
            {
                return result;
            }

            var side = direction == Direction.BUY
                ? CryptoExchange.Net.CommonObjects.CommonOrderSide.Buy
                : CryptoExchange.Net.CommonObjects.CommonOrderSide.Sell;

            var type = orderType == OrderType.Limit
                ? CryptoExchange.Net.CommonObjects.CommonOrderType.Limit
                : CryptoExchange.Net.CommonObjects.CommonOrderType.Market;

            if (orderType == OrderType.Market)
            {
                price = null;
            }

            var newOrder = await _client.PlaceOrderAsync(
                symbol: instrument.ToUpper(),
                side: side,
                type: type,
                quantity: volume,
                price: price,
                clientOrderId: orderId);

            if (newOrder.Success && newOrder.Data != null)
            {
                if (_orderInternalIdsDictionary.TryAdd(newOrder.Data.Id, orderId))
                {
                    _logger.LogTrace($"MakeOrderAsync added {newOrder.Data.Id} _ordersGuids count:{_orderInternalIdsDictionary.Count} {StockName} {instrument}");
                }
                else
                {
                    _logger.LogWarning($"MakeOrderAsync TryAdd false, OrderId:{newOrder.Data.Id} count:{_orderInternalIdsDictionary.Count} {StockName} {instrument}");
                }

                result.Success = true;
                result.Price = price.GetValueOrDefault();
                result.Volume = volume;
                result.StockOrderId = newOrder.Data.Id;
                result.TimeStamp = DateTime.UtcNow;
                result.SystemOrderId = Guid.Parse(orderId);
            }
            else
            {
                result.ErrorType = BaseOrderModel.OrderErrorType.Unknown;
                result.Error = newOrder.Error?.ToString() ?? $"{StockName} error";

                if (newOrder.ResponseStatusCode.HasValue && (int)newOrder.ResponseStatusCode >= 500 && (int)newOrder.ResponseStatusCode < 600)
                {
                    result.ErrorType = BaseOrderModel.OrderErrorType.ServiceUnavailable;
                }

                if (result.Error.Contains("Too many") || result.Error.Contains("rate limit"))
                {
                    result.ErrorType = BaseOrderModel.OrderErrorType.ServiceUnavailable;
                }
            }

            return result;
        }
        
        public async Task<List<OrderHistoryItem>> GetOrderHistory(string instrumentName, DateTime fromDateUtc, DateTime toDateUtc, InstrumentType instrumentType)
        {
            // FameEX doesn't have order history endpoint in basic docs
            throw new NotImplementedException("GetOrderHistory not available in FameEX API");
        }

        public async Task<IList<BaseOrderModel>> MakeBatchOrdersAsync(InstrumentType instrumentType, IList<OrderCreateRequest> orders)
        {
            var result = new List<BaseOrderModel>();
            foreach (var order in orders)
            {
                var res = await MakeOrderAsync(instrumentType, order.Type,
                    order.OrderDirection, order.PositionType, order.Instrument,
                    order.Volume, order.Price, order.ClientOrderId);
                result.Add(res);
            }
            return result;
        }

        public async Task<BalancesModel> GetBalance(string coinName, BalanceType balanceType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var balance = await _client.Account.GetAccountInfoAsync();
            if (!balance.Success)
            {
                return new BalancesModel { StockName = StockName, Balances = new List<Balance>() };
            }

            var coinBalance = balance.Data.Balances.FirstOrDefault(x => x.Asset.Equals(coinName, StringComparison.OrdinalIgnoreCase));

            decimal.TryParse(coinBalance?.Free, NumberStyles.Any, CultureInfo.InvariantCulture, out var free);
            decimal.TryParse(coinBalance?.Locked, NumberStyles.Any, CultureInfo.InvariantCulture, out var locked);

            return new BalancesModel
            {
                StockName = StockName,
                Balances = new List<Balance>
        {
            new Balance
            {
                Currency = coinName,
                AvalibleBalance = free,
                FreezedBalance = locked,
            }
        }
            };
        }

        public async Task<BalancesModel> GetBalanceMain(string coinName)
        {
            var balanceData = await GetBalance(coinName, BalanceType.Spot);
            return balanceData;
        }

        public async Task<List<AssetBalance>> GetAllBalances(BalanceType balanceType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var accountInfo = await _client.Account.GetAccountInfoAsync();
            if (!accountInfo.Success)
            {
                throw new Exception($"{accountInfo.Error}");
            }

            var coinBalances = accountInfo.Data.Balances;

            return coinBalances
                .Where(e =>
                {
                    decimal.TryParse(e.Free, NumberStyles.Any, CultureInfo.InvariantCulture, out var free);
                    decimal.TryParse(e.Locked, NumberStyles.Any, CultureInfo.InvariantCulture, out var locked);
                    return free != 0 || locked != 0;
                })
                .Select(e =>
                {
                    decimal.TryParse(e.Free, NumberStyles.Any, CultureInfo.InvariantCulture, out var free);
                    decimal.TryParse(e.Locked, NumberStyles.Any, CultureInfo.InvariantCulture, out var locked);

                    return new AssetBalance
                    {
                        Currency = e.Asset,
                        AvailableBalance = free,
                        FrozenBalance = locked
                    };
                }).ToList();
        }

        //public async Task<ExchangeResponse<List<MarketTickerModel>>> FetchTickersAsync()
        //{
        //    if (_client == null)
        //    {
        //        throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
        //    }

        //    var symbols = await _client.GetSymbolsAsync();
        //    if (!symbols.Success)
        //    {
        //        return ExchangeResponse<List<MarketTickerModel>>.Failed(symbols.OriginalData);
        //    }

        //    var result = new List<MarketTickerModel>();

        //    // FameEX doesn't have bulk ticker endpoint, need to fetch individually or use alternative approach
        //    // This is a placeholder - you may need to adjust based on actual API capabilities

        //    return ExchangeResponse<List<MarketTickerModel>>.Success(result, symbols.OriginalData);
        //}

        public Task<ExchangeResponse<OptionTickerModel>> GetOptionTicker(string instrumentName)
        {
            throw new NotImplementedException();
        }

        public Task<List<OrderHistoryItem>> GetOrderHistoryForPeriodWithFee(string instrumentName, DateTime fromDateUtc, DateTime toDateUtc, InstrumentType instrumentType)
        {
            throw new NotImplementedException();
        }

        #endregion PrivateApi

        #region deposit/withdraw/transfers

        public Task<ExchangeResponse<List<DepositItemModel>>> GetDepositHistory(string asset,
            DateTime? dateFromUtc = null, int? limit = null)
        {
            throw new NotImplementedException();
        }

        public Task<ExchangeResponse<List<WithdrawalItemModel>>> GetWithdrawHistory(string asset,
            DateTime? dateFromUtc = null, int? limit = null)
        {
            throw new NotImplementedException();
        }

        public Task<ExchangeResponse<List<TransferItemModel>>> GetTransferHistory(string asset, int limit,
            DateTime? dateFromUtc = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region NotImportant

        public Task<List<BaseInstrumentModel>> GetInstrumetsAsync(InstrumentType instrumentType)
        {
            throw new NotImplementedException();
        }

        public Task<BaseOrderModel> ClosePositionAsync(string instrumentName, string orderId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetLeverage(string instrumentName, int leverage)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetLeverage(List<string> instrumentNames, int leverage)
        {
            throw new NotImplementedException();
        }

        public Task<Withdraw> Withdrawal(string instrumentName, decimal amount, string address, string memo, string uniqueid, string network = "")
        {
            throw new NotImplementedException();
        }

        public Task<WithdrawHistory> WithdrawalHistory(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, List<OrderTrade>>> GetTradeHistoryByOrders(string instrumentName, List<string> orderIds, DateTime fromDateUtc, DateTime toDateUtc)
        {
            throw new NotImplementedException();
        }

        public Task<List<OrderTrade>> GetTradeHistory(InstrumentType instrumentType, string instrumentName, DateTime fromDateUtc, DateTime toDateUtc)
        {
            throw new NotImplementedException();
        }

        #endregion

        public async Task<SocketListenKey> GetListenKey(InstrumentType instrumentType)
        {
            // FameEX doesn't use listen keys for websocket authentication
            return new SocketListenKey { ListenKey = string.Empty };
        }

        public async Task<SocketKeepAliveListenKey> KeepAlive(InstrumentType instrumentType, string listenKey)
        {
            // FameEX doesn't use listen keys for websocket authentication
            return new SocketKeepAliveListenKey();
        }

        private OrderHistoryItemStatus ConvertOrderStatus(CryptoExchange.Net.CommonObjects.CommonOrderStatus status)
        {
            return status switch
            {
                CryptoExchange.Net.CommonObjects.CommonOrderStatus.Active => OrderHistoryItemStatus.Open,
                CryptoExchange.Net.CommonObjects.CommonOrderStatus.Filled => OrderHistoryItemStatus.Filled,
                CryptoExchange.Net.CommonObjects.CommonOrderStatus.Canceled => OrderHistoryItemStatus.Canceled,
                _ => OrderHistoryItemStatus.Open
            };
        }

        public async Task DisconnectAsync()
        {
            await Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
            _client?.Dispose();
        }

        public Task<FundingRate?> GetFundingRate(string instrumentName)
        {
            throw new NotImplementedException();
        }

        public Task<Commission?> GetAccountCommissionAsync(string instrumentName, InstrumentType instrumentType)
        {
            throw new NotImplementedException();
        }

        public Task<List<OptionInfo>> GetOptionList(string instrumentName, DateTime expirationDate)
        {
            throw new NotImplementedException();
        }

        public Task<List<Position>> GetPositionsAsync(string currency, InstrumentType instrumentType = InstrumentType.Option)
        {
            throw new NotImplementedException();
        }

        //public async Task<RateLimitSnapshot> GetRateLimitSnapshot()
        //{
        //    if (_client == null)
        //    {
        //        throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
        //    }

        //    var snapshot = await _client.GetCurrentRateLimitsAsync();
        //    snapshot.ExchangeName = StockName;
        //    return snapshot;
        //}

        private Guid GetInternalOrderGuid(string externalOrderId)
        {
            if (_orderInternalIdsDictionary.TryGetValue(externalOrderId, out var internalId))
            {
                return Guid.Parse(internalId);
            }

            return Guid.Empty;
        }

        public Task<ExchangeResponse<List<MarketTickerModel>>> FetchTickersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<RateLimitSnapshot> GetRateLimitSnapshot()
        {
            throw new NotImplementedException();
        }
    }
}