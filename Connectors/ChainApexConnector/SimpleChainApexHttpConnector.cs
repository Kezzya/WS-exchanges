using System.Text.Json;
using System.Collections.Concurrent;
using System.Net;
using System.Globalization;
using Microsoft.Extensions.Logging;
using BaseStockConnectorInterface.Models;
using BaseStockConnectorInterface.Models.Instruments;
using BaseStockConnectorInterface.Models.Orders;
using BaseStockConnectorInterface.Models.History;
using BaseStockConnectorInterface;
using BaseStockConnector;
using BaseStockConnector.Models.Enums;
using BaseStockConnector.Models.Instruments;
using BaseStockConnector.Models.Orders;
using BaseStockConnectorInterface.Models.Enums;
using BaseStockConnectorInterface.Models.Kline;
using BaseStockConnectorInterface.Models.Ticker;
using BaseStockConnectorInterface.Logger;
using BaseStockConnectorInterface.Helper;
using ChainApex.Net;
using ChainApex.Net.Clients;
using ChainApex.Net.Objects.Models.ExchangeData;
using CryptoExchange.Net.RateLimiting;
using StockConnector;
using Direction = BaseStockConnector.Models.Enums.Direction;

namespace ChainApexConnector
{
    internal class SimpleChainApexHttpConnector : IStockHttp
    {
        private const string StockName = "ChainApex";
        
        public ChainApexRestClient _client;
        private ILogger _logger;
        private readonly ChainApexSocketConnector _connector;
        private ConcurrentDictionary<string, string> _orderInternalIdsDictionary;
        private string? _customRestUrl;
        private string? _customWebSocketUrl;

        public event Action<RateLimitEvent> RateLimitTriggered;

        public SimpleChainApexHttpConnector(ChainApexSocketConnector connector, ConcurrentDictionary<string, string> orderInternalIdsDictionary)
        {
            _connector = connector;
            _orderInternalIdsDictionary = orderInternalIdsDictionary;
        }

        public void SetCustomUrls(string? restUrl, string? webSocketUrl)
        {
            _customRestUrl = restUrl;
            _customWebSocketUrl = webSocketUrl;
        }

        public async Task<RateLimitSnapshot> GetRateLimitSnapshot()
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var snapshot = await _client.GetCurrentRateLimitsAsync();
            snapshot.ExchangeName = StockName;
            return snapshot;
        }

        public async Task ConnectAsync(IStockCredential stockCredential, ILogger clientLogger, List<WebProxy>? proxies, HttpClientHandler? httpClientHandler)
        {
            if (_client != null)
            {
                return;
            }

            ChainApexCredential credential;
            _logger = clientLogger;

            try
            {
                credential = (ChainApexCredential)stockCredential;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }

            var logFactory = new LoggerFactory();
            logFactory.AddProvider(new ExchangeLoggerProvider(new PrefixedLogger(_logger, StockName)));

            var httpClient = new HttpClient();

            if (httpClientHandler != null)
            {
                httpClient = new HttpClient(httpClientHandler);
            }

            _client = new ChainApexRestClient(httpClient, logFactory, options =>
            {
                options.OutputOriginalData = true;
                options.ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials(credential.ApiKey, credential.Secret);
                options.RateLimiterEnabled = true;
                
                // Используем кастомное окружение если указаны URL
                if (!string.IsNullOrEmpty(_customRestUrl) || !string.IsNullOrEmpty(_customWebSocketUrl))
                {
                    options.Environment = ChainApexEnvironment.CreateCustom(
                        "Custom",
                        _customRestUrl ?? "https://openapi.chainapex.pro",
                        _customWebSocketUrl ?? "wss://wspool.hiotc.pro/kline-api/ws");
                }
            });

            await _connector.ConnectAsync(credential, _logger, null);

            await Task.CompletedTask;
        }

        public async Task ConnectAsync(IStockCredential stockCredential, ILogger clientLogger, WebProxy proxy)
        {
            if (_client != null)
            {
                return;
            }

            ChainApexCredential credential;
            _logger = clientLogger;
            try
            {
                credential = (ChainApexCredential)stockCredential;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }

            var logFactory = new LoggerFactory();
            logFactory.AddProvider(new ExchangeLoggerProvider(new PrefixedLogger(_logger, StockName)));

            _client = new ChainApexRestClient(null, logFactory, options =>
            {
                options.OutputOriginalData = true;
                options.ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials(credential.ApiKey, credential.Secret);
                options.RateLimiterEnabled = true;
                
                // Используем кастомное окружение если указаны URL
                if (!string.IsNullOrEmpty(_customRestUrl) || !string.IsNullOrEmpty(_customWebSocketUrl))
                {
                    options.Environment = ChainApexEnvironment.CreateCustom(
                        "Custom",
                        _customRestUrl ?? "https://openapi.chainapex.pro",
                        _customWebSocketUrl ?? "wss://wspool.hiotc.pro/kline-api/ws");
                }
                
                if (proxy != null)
                {
                    var host = $"{proxy.Address!.Scheme}://{proxy.Address!.Host}";
                    var port = proxy.Address!.Port;
                    options.Proxy = new CryptoExchange.Net.Objects.ApiProxy(host, port);
                }
            });

            await _connector.ConnectAsync(credential, _logger, null);

            await Task.CompletedTask;
        }

        #region PublicApi

        public async Task<DateTime> GetServerTimestamp()
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var timestamp = await _client.SpotApi.ExchangeData.GetServerTimeAsync();

            if (timestamp.Success)
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(timestamp.Data.ServerTime).DateTime;
            }

            return DateTime.UtcNow;
        }

        public async Task<SpotCoinModel> GetCoinInfoAsync(string instrumentName, InstrumentType instrumentType)
        {
            // For simplicity, return a basic coin info
            return new SpotCoinModel
            {
                InstrumentName = instrumentName,
                OriginalInstrumentName = instrumentName,
                StockName = "ChainApex",
                BaseCurrency = instrumentName.Substring(0, 3),
                QuoteCurrency = instrumentName.Substring(3)
            };
        }

        public async Task<List<SpotCoinModel>> GetCoinInfoAsync(InstrumentType instrumentType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            if (instrumentType != InstrumentType.Spot)
            {
                return new List<SpotCoinModel>();
            }

            var symbolsResult = await _client.SpotApi.ExchangeData.GetSymbolsAsync();

            if (symbolsResult.Success && symbolsResult.Data?.Symbols != null)
            {
                return symbolsResult.Data.Symbols.Select(s => new SpotCoinModel
                {
                    InstrumentName = s.Symbol,
                    OriginalInstrumentName = s.Symbol,
                    StockName = StockName,
                    BaseCurrency = s.BaseAsset,
                    QuoteCurrency = s.QuoteAsset,
                    MinVolumeInBaseCurrency = s.LimitVolumeMin,
                    MinMovement = s.LimitPriceMin > 0 ? s.LimitPriceMin : 0.01m,
                    MinMovementVolume = s.LimitVolumeMin > 0 ? s.LimitVolumeMin : 0.00001m
                }).ToList();
            }

            return new List<SpotCoinModel>();
        }

        public async Task<decimal> Get24hVolume(string coinName, InstrumentType instrumentType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            if (instrumentType != InstrumentType.Spot)
            {
                return 0;
            }

            var tickerResult = await _client.SpotApi.ExchangeData.GetTickerAsync(coinName.ToUpper());

            if (tickerResult.Success && tickerResult.Data != null)
            {
                if (decimal.TryParse(tickerResult.Data.Volume, NumberStyles.Float, CultureInfo.InvariantCulture, out var volume))
                {
                    return volume;
                }
            }

            return 0;
        }

        public async Task<decimal> Get24hVolume(string coinName, DateTime date, InstrumentType instrumentType = InstrumentType.Spot)
        {
            // ChainApex doesn't support historical volume, return current volume
            return await Get24hVolume(coinName, instrumentType);
        }

        public async Task<OrderBook> GetDepthAsync(InstrumentType instrumentType, string symbol, int depth)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            if (instrumentType != InstrumentType.Spot)
            {
                return new OrderBook { InstrumentName = symbol, StockName = StockName };
            }

            var orderBookResult = await _client.SpotApi.ExchangeData.GetOrderBookAsync(symbol.ToUpper(), depth);

            if (orderBookResult.Success && orderBookResult.Data != null)
            {
                    return new OrderBook
                    {
                        InstrumentName = symbol,
                        OriginalInstrumentName = symbol,
                    StockName = StockName,
                    Bids = orderBookResult.Data.Bids?.Select(bid => new OrderBookEntry
                    {
                        Price = decimal.Parse(bid[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                        Amount = decimal.Parse(bid[1], NumberStyles.Float, CultureInfo.InvariantCulture)
                    }).ToList() ?? new List<OrderBookEntry>(),
                    Asks = orderBookResult.Data.Asks?.Select(ask => new OrderBookEntry
                    {
                        Price = decimal.Parse(ask[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                        Amount = decimal.Parse(ask[1], NumberStyles.Float, CultureInfo.InvariantCulture)
                    }).ToList() ?? new List<OrderBookEntry>()
                    };
                }
                
            return new OrderBook { InstrumentName = symbol, StockName = StockName };
        }

        public async Task<List<KlineItem>> GetKlines(string instrumentName, KlineInterval klineInterval, DateTime? fromDateUtc, DateTime? toDateUtc, int? limit = null, InstrumentType instrumentType = InstrumentType.Spot)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            if (instrumentType != InstrumentType.Spot)
            {
                return new List<KlineItem>();
            }

            var chainApexInterval = klineInterval switch
            {
                KlineInterval.OneMinute => ChainApexKlineInterval.OneMinute,
                KlineInterval.FiveMinutes => ChainApexKlineInterval.FiveMinutes,
                KlineInterval.FifteenMinutes => ChainApexKlineInterval.FifteenMinutes,
                KlineInterval.ThirtyMinutes => ChainApexKlineInterval.ThirtyMinutes,
                KlineInterval.OneHour => ChainApexKlineInterval.OneHour,
                KlineInterval.FourHour => ChainApexKlineInterval.FourHours,
                KlineInterval.OneDay => ChainApexKlineInterval.OneDay,
                KlineInterval.OneMonth => ChainApexKlineInterval.OneWeek, // Map OneMonth to OneWeek as closest match
                _ => ChainApexKlineInterval.OneMinute
            };

            var klinesResult = await _client.SpotApi.ExchangeData.GetKlinesAsync(
                instrumentName.ToUpper(), 
                chainApexInterval, 
                fromDateUtc, 
                toDateUtc, 
                limit);

            if (klinesResult.Success && klinesResult.Data != null)
            {
                return klinesResult.Data.Select(k => new KlineItem
                {
                    OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(k.Idx).DateTime,
                    OpenPrice = decimal.Parse(k.Open, NumberStyles.Float, CultureInfo.InvariantCulture),
                    HighPrice = decimal.Parse(k.High, NumberStyles.Float, CultureInfo.InvariantCulture),
                    LowPrice = decimal.Parse(k.Low, NumberStyles.Float, CultureInfo.InvariantCulture),
                    ClosePrice = decimal.Parse(k.Close, NumberStyles.Float, CultureInfo.InvariantCulture),
                    Volume = decimal.Parse(k.Volume, NumberStyles.Float, CultureInfo.InvariantCulture)
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

            if (instrumentType != InstrumentType.Spot)
            {
                return new OrderInfo { InstrumentName = instrumentName, StockName = StockName };
            }

            var tradesResult = await _client.SpotApi.ExchangeData.GetRecentTradesAsync(instrumentName.ToUpper(), 1);

            if (tradesResult.Success && tradesResult.Data != null && tradesResult.Data.Any())
            {
                var trade = tradesResult.Data.First();
                        return new OrderInfo
                        {
                            InstrumentName = instrumentName,
                            OriginalInstrumentName = instrumentName,
                    StockName = StockName,
                            Price = trade.Price,
                    Volume = trade.Qty,
                    Direction = trade.Side.ToLower() == "buy" ? Direction.BUY : Direction.SELL,
                    TransactionDate = DateTimeOffset.FromUnixTimeMilliseconds(trade.Time).DateTime
                };
            }

            return new OrderInfo { InstrumentName = instrumentName, StockName = StockName };
        }

        #endregion

        #region PrivateApi

        public async Task<List<BaseOrderModel>> GetActiveOrdersAsync(string instrumentName, InstrumentType instrumentType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            if (instrumentType != InstrumentType.Spot)
            {
                return new List<BaseOrderModel>();
            }

            var ordersResult = await _client.SpotApi.Account.GetOpenOrdersAsync(instrumentName.ToUpper());

            if (ordersResult.Success && ordersResult.Data != null)
            {
                return ordersResult.Data.Select(order => new BaseOrderModel
                {
                    StockOrderId = order.OrderId?.ToString() ?? "",
                    InstrumentName = instrumentName,
                    OriginalInstrumentName = instrumentName,
                    StockName = StockName,
                    Price = order.Price,
                    Volume = order.OrigQty,
                    FilledVolume = order.ExecutedQty,
                    Direction = order.Side.ToLower() == "buy" ? Direction.BUY : Direction.SELL,
                    Type = order.Type.ToLower() == "limit" ? OrderType.Limit : OrderType.Market,
                    Success = IsOrderSuccessful(order.Status),
                    TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(order.TransactTime).DateTime
                }).ToList();
            }

            return new List<BaseOrderModel>();
        }

        public async Task<OrderSyncInfo?> GetOrderInfo(string instrumentName, string orderId, InstrumentType instrumentType = InstrumentType.Spot)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            if (instrumentType != InstrumentType.Spot)
            {
                return null;
            }

            var orderResult = await _client.SpotApi.Account.GetOrderAsync(instrumentName.ToUpper(), orderId);

            if (orderResult.Success && orderResult.Data != null)
            {
                var order = orderResult.Data;
                return new OrderSyncInfo
                {
                    StockOrderId = order.OrderId?.ToString() ?? "",
                    InstrumentName = instrumentName,
                    Price = order.Price,
                    Volume = order.OrigQty,
                    FilledVolume = order.ExecutedQty,
                    Direction = order.Side.ToLower() == "buy" ? Direction.BUY : Direction.SELL,
                    OrderType = order.Type.ToLower() == "limit" ? OrderType.Limit : OrderType.Market,
                    Status = ConvertOrderStatusToHistoryStatus(order.Status),
                    DateCreated = DateTimeOffset.FromUnixTimeMilliseconds(order.TransactTime).DateTime
                };
            }

            return null;
        }

        public async Task<BaseOrderModel> MakeOrderAsync(InstrumentType instrumentType, OrderType orderType, Direction direction, PositionOrderType positionOrderType, string instrument, decimal volume, decimal? price, string orderId)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            if (instrumentType != InstrumentType.Spot)
            {
                return new BaseOrderModel { Success = false, Error = "Only Spot orders supported" };
            }

            var side = direction == Direction.BUY ? "BUY" : "SELL";
            var type = orderType == OrderType.Limit ? "LIMIT" : "MARKET";

            var orderResult = await _client.SpotApi.Account.PlaceOrderAsync(
                instrument.ToUpper(),
                side,
                type,
                volume,
                price);

            if (orderResult.Success && orderResult.Data != null)
            {
                var stockOrderId = orderResult.Data.OrderId?.ToString() ?? "";
                _orderInternalIdsDictionary.TryAdd(orderId, stockOrderId);

                    return new BaseOrderModel
                    {
                        StockOrderId = stockOrderId,
                        InstrumentName = instrument,
                        OriginalInstrumentName = instrument,
                        StockName = StockName,
                        Price = orderResult.Data.Price,
                        Volume = orderResult.Data.OrigQty,
                        FilledVolume = orderResult.Data.ExecutedQty,
                        Direction = direction,
                        Type = orderType,
                        Success = IsOrderSuccessful(orderResult.Data.Status),
                        TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(orderResult.Data.TransactTime).DateTime
                    };
            }

            return new BaseOrderModel
            {
                Success = false,
                Error = orderResult.Error?.Message ?? "Unknown error"
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
                return new BaseOrderModel { Success = false, Error = "Only Spot orders supported" };
            }

            // Try to get internal order ID
            if (_orderInternalIdsDictionary.TryGetValue(orderId, out var internalOrderId))
            {
                orderId = internalOrderId;
            }

            var cancelResult = await _client.SpotApi.Account.CancelOrderAsync(instrumentName.ToUpper(), orderId);

            if (cancelResult.Success && cancelResult.Data != null)
            {
                return new BaseOrderModel
                {
                    StockOrderId = cancelResult.Data.OrderId?.ToString() ?? "",
                    InstrumentName = instrumentName,
                    OriginalInstrumentName = instrumentName,
                    StockName = StockName,
                    Success = true
                };
            }

            return new BaseOrderModel
            {
                Success = false,
                Error = cancelResult.Error?.Message ?? "Unknown error"
            };
        }

        public async Task<BalancesModel> GetBalance(string coinName, BalanceType balanceType = BalanceType.Spot)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var accountResult = await _client.SpotApi.Account.GetAccountInfoAsync();

            if (accountResult.Success && accountResult.Data?.Balances != null)
            {
                var balance = accountResult.Data.Balances.FirstOrDefault(b => 
                    b.Asset.Equals(coinName, StringComparison.OrdinalIgnoreCase));

                if (balance != null)
                {
                    return new BalancesModel
                    {
                        StockName = StockName,
                        Balances = new List<Balance>
                        {
                            new Balance
                            {
                                Currency = balance.Asset,
                                AvalibleBalance = decimal.Parse(balance.Free, NumberStyles.Float, CultureInfo.InvariantCulture),
                                FreezedBalance = decimal.Parse(balance.Locked, NumberStyles.Float, CultureInfo.InvariantCulture)
                            }
                        }
                    };
                }
            }

            return new BalancesModel 
            { 
                StockName = StockName,
                Balances = new List<Balance>()
            };
        }

        public async Task<List<AssetBalance>> GetAllBalances(BalanceType balanceType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var accountResult = await _client.SpotApi.Account.GetAccountInfoAsync();

            if (accountResult.Success && accountResult.Data?.Balances != null)
            {
                return accountResult.Data.Balances
                    .Where(b => 
                    {
                        // Используем доступные поля для определения баланса
                        var free = !string.IsNullOrEmpty(b.Free) ? decimal.Parse(b.Free, NumberStyles.Float, CultureInfo.InvariantCulture) : 0;
                        var locked = !string.IsNullOrEmpty(b.Locked) ? decimal.Parse(b.Locked, NumberStyles.Float, CultureInfo.InvariantCulture) : 0;
                        var available = !string.IsNullOrEmpty(b.Available) ? decimal.Parse(b.Available, NumberStyles.Float, CultureInfo.InvariantCulture) : free;
                        var total = !string.IsNullOrEmpty(b.Total) ? decimal.Parse(b.Total, NumberStyles.Float, CultureInfo.InvariantCulture) : free + locked;
                        
                        return available > 0 || locked > 0;
                    })
                    .Select(b => new AssetBalance
                    {
                        Currency = b.Asset,
                        AvailableBalance = !string.IsNullOrEmpty(b.Available) ? 
                            decimal.Parse(b.Available, NumberStyles.Float, CultureInfo.InvariantCulture) :
                            decimal.Parse(b.Free, NumberStyles.Float, CultureInfo.InvariantCulture),
                        FrozenBalance = decimal.Parse(b.Locked, NumberStyles.Float, CultureInfo.InvariantCulture)
                    }).ToList();
            }

            return new List<AssetBalance>();
        }

        public async Task<List<OrderHistoryItem>> GetOrderHistoryForPeriodWithFee(string instrumentName, DateTime fromDateUtc, DateTime toDateUtc, InstrumentType instrumentType = InstrumentType.Spot)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            if (instrumentType != InstrumentType.Spot)
            {
                return new List<OrderHistoryItem>();
            }

            var tradesResult = await _client.SpotApi.Account.GetMyTradesAsync(instrumentName.ToUpper());

            if (tradesResult.Success && tradesResult.Data != null)
            {
                return tradesResult.Data
                    .Where(t => {
                        var time = DateTimeOffset.FromUnixTimeMilliseconds(t.Time).DateTime;
                        return time >= fromDateUtc && time <= toDateUtc;
                    })
                    .Select(t => new OrderHistoryItem
                    {
                        StockOrderId = t.Id.ToString(),
                        InstrumentName = instrumentName,
                        Price = decimal.Parse(t.Price, NumberStyles.Float, CultureInfo.InvariantCulture),
                        Volume = decimal.Parse(t.Qty, NumberStyles.Float, CultureInfo.InvariantCulture),
                        FilledVolume = decimal.Parse(t.Qty, NumberStyles.Float, CultureInfo.InvariantCulture), // Traded volume is filled volume
                        Direction = t.IsBuyer ? Direction.BUY : Direction.SELL,
                        Fee = decimal.Parse(t.Fee, NumberStyles.Float, CultureInfo.InvariantCulture),
                        FeeCurrency = t.FeeCoin,
                        DateCreated = DateTimeOffset.FromUnixTimeMilliseconds(t.Time).DateTime,
                        Status = OrderHistoryItemStatus.Filled // Trades are always filled
                    }).ToList();
            }

            return new List<OrderHistoryItem>();
        }

        private OrderHistoryItemStatus ConvertOrderStatusToHistoryStatus(string status)
        {
            return status.ToUpper() switch
            {
                "NEW" => OrderHistoryItemStatus.Open,
                "PARTIALLY_FILLED" => OrderHistoryItemStatus.PartiallyFilled,
                "FILLED" => OrderHistoryItemStatus.Filled,
                "CANCELED" => OrderHistoryItemStatus.Canceled,
                "PENDING_CANCEL" => OrderHistoryItemStatus.Open,
                "REJECTED" => OrderHistoryItemStatus.Canceled,
                "EXPIRED" => OrderHistoryItemStatus.Canceled,
                _ => OrderHistoryItemStatus.None
            };
        }

        private static bool IsOrderSuccessful(string status)
        {
            return status.ToUpper() switch
            {
                "NEW" => true,                          // New Order
                "PARTIALLY_FILLED" => true,            // Partially Filled
                "FILLED" => true,                      // Filled
                "CANCELED" => false,                   // Cancelled
                "CANCELLED" => false,                  // Cancelled (alternative spelling)
                "TO_BE_CANCELLED" => false,           // To be Cancelled
                "PARTIALLY_FILLED_CANCELLED" => false, // Partially Filled/Cancelled
                "REJECTED" => false,                   // REJECTED
                "PENDING_CANCEL" => false,             // Pending Cancel
                "EXPIRED" => false,                    // Expired
                _ => false
            };
        }

        #endregion

        #region NotImplemented

        public Task<BaseOrderModel> ClosePositionAsync(string instrumentName, string orderId)
        {
            throw new NotImplementedException("ChainApex does not support positions");
        }

        public Task<bool> SetLeverage(string instrumentName, int leverage)
        {
            throw new NotImplementedException("ChainApex does not support leverage");
        }

        public Task<bool> SetLeverage(List<string> instrumentNames, int leverage)
        {
            throw new NotImplementedException("ChainApex does not support leverage");
        }

        public Task<List<OrderHistoryItem>> GetOrderHistory(string instrumentName, DateTime fromDateUtc, DateTime toDateUtc, InstrumentType instrumentType = InstrumentType.Spot)
        {
            // Redirect to the method with fees
            return GetOrderHistoryForPeriodWithFee(instrumentName, fromDateUtc, toDateUtc, instrumentType);
        }

        public Task<Dictionary<string, List<OrderTrade>>> GetTradeHistoryByOrders(string instrumentName, List<string> orderIds, DateTime fromDateUtc, DateTime toDateUtc)
        {
            throw new NotImplementedException("GetTradeHistoryByOrders not implemented for ChainApex");
        }

        public Task<List<OrderTrade>> GetTradeHistory(InstrumentType instrumentType, string instrumentName, DateTime fromDateUtc, DateTime toDateUtc)
        {
            throw new NotImplementedException("GetTradeHistory not implemented for ChainApex");
        }

        public Task<List<Position>> GetPositionsAsync(string currency, InstrumentType instrumentType = InstrumentType.Option)
        {
            throw new NotImplementedException("ChainApex does not support positions");
        }

        public Task<List<BaseInstrumentModel>> GetInstrumetsAsync(InstrumentType instrumentType)
        {
            throw new NotImplementedException("GetInstrumetsAsync not implemented for ChainApex");
        }

        public Task<ExchangeResponse<List<OptionInfo>>> GetOptionList(string? instrumentName, DateTime? expirationDate)
        {
            throw new NotImplementedException("ChainApex does not support options");
        }

        public Task<ExchangeResponse<List<Volatility>>> GetVolatility(string instrumentName, DateTime from, DateTime to)
        {
            throw new NotImplementedException("ChainApex does not support volatility data");
        }

        public Task<BalancesModel> GetBalanceMain(string coinName)
        {
            // ChainApex only has spot balance
            return GetBalance(coinName, BalanceType.Spot);
        }

        public Task<BalanceTransfer> Transfer(AccountType from, AccountType to, string instrumentName, decimal amount)
        {
            throw new NotImplementedException("ChainApex does not support transfers");
        }

        public Task<Withdraw> Withdrawal(string instrumentName, decimal amount, string address, string memo, string uniqueid, string network = "")
        {
            throw new NotImplementedException("Withdrawal not implemented for ChainApex");
        }

        public Task<WithdrawHistory> WithdrawalHistory(string id)
        {
            throw new NotImplementedException("WithdrawalHistory not implemented for ChainApex");
        }

        public Task<SocketListenKey> GetListenKey(InstrumentType instrumentType)
        {
            throw new NotImplementedException("GetListenKey not implemented for ChainApex");
        }

        public Task<SocketKeepAliveListenKey> KeepAlive(InstrumentType instrumentType, string listenKey)
        {
            throw new NotImplementedException("KeepAlive not implemented for ChainApex");
        }

        public Task<FundingRate?> GetFundingRate(string instrumentName)
        {
            throw new NotImplementedException("ChainApex does not support funding rates");
        }

        public Task<ExchangeResponse<List<MarketTickerModel>>> FetchTickersAsync()
        {
            throw new NotImplementedException("FetchTickersAsync not implemented for ChainApex");
        }

        public Task<ExchangeResponse<OptionTickerModel>> GetOptionTicker(string instrumentName)
        {
            throw new NotImplementedException("ChainApex does not support options");
        }

        public Task<Commission?> GetAccountCommissionAsync(string instrumentName, InstrumentType instrumentType)
        {
            throw new NotImplementedException("GetAccountCommissionAsync not implemented for ChainApex");
        }

        public Task<IList<BaseOrderModel>> MakeBatchOrdersAsync(InstrumentType instrumentType, IList<OrderCreateRequest> orders)
        {
            throw new NotImplementedException("Batch orders not implemented for ChainApex");
        }

        public async Task DisconnectAsync()
        {
            if (_connector != null)
            {
                await _connector.DisconnectAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
            _client?.Dispose();
        }

        #endregion
    }
}
