using System.Collections.Concurrent;
using System.Net;
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
using Microsoft.Extensions.Logging;
using Digifinex.Net.Clients;
using Digifinex.Net.Objects.Models.Account;
using Digifinex.Net.Objects.Models.ExchangeData;
using StockConnector;
using AccountType = BaseStockConnectorInterface.Models.Enums.AccountType;
using DigifinexOrderType = Digifinex.Net.Objects.Models.Account.DigifinexOrderType;
using OrderType = BaseStockConnector.Models.Enums.OrderType;
using WithdrawHistory = BaseStockConnectorInterface.Models.WithdrawHistory;

namespace DigifinexConnector
{
    internal class DigifinexHttpConnector : IStockHttp
    {
        private const string StockName = "Digifinex";

        public DigifinexRestClient _client;

        private ILogger _logger;
        private readonly DigifinexSocketConnector _connector;

        public event Action<RateLimitEvent> RateLimitTriggered;

        private ConcurrentDictionary<string, string> _orderInternalIdsDictionary;

        public DigifinexHttpConnector(DigifinexSocketConnector connector, ConcurrentDictionary<string, string> orderInternalIdsDictionary)
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

            DigifinexCredential credential;
            _logger = clientLogger;

            try
            {
                credential = (DigifinexCredential)stockCredential;
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

            _client = new DigifinexRestClient(httpClient, logFactory, options =>
            {
                options.OutputOriginalData = true;
                options.ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials(credential.ApiKey, credential.Secret);
                options.RateLimiterEnabled = true;
            });

            await _connector.ConnectAsync(credential, _logger, null);
            _client.SpotApi.Account.AddSocket(_connector.ClientSocket.SpotApi);

            await Task.CompletedTask;
        }

        public async Task ConnectAsync(IStockCredential stockCredential, ILogger clientLogger, WebProxy proxy)
        {
            if (_client != null)
            {
                return;
            }

            DigifinexCredential credential;
            _logger = clientLogger;
            try
            {
                credential = (DigifinexCredential)stockCredential;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }

            var logFactory = new LoggerFactory();
            logFactory.AddProvider(new ExchangeLoggerProvider(new PrefixedLogger(_logger, StockName)));

            _client = new DigifinexRestClient(null, logFactory, options =>
            {
                options.OutputOriginalData = true;
                options.ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials(credential.ApiKey, credential.Secret);
                options.RateLimiterEnabled = true;
                if (proxy != null)
                {
                    var host = $"{proxy.Address!.Scheme}://{proxy.Address!.Host}";
                    var port = proxy.Address!.Port;
                    options.Proxy = new CryptoExchange.Net.Objects.ApiProxy(host, port);
                }
            });

            await _connector.ConnectAsync(credential, _logger, null);
            _client.SpotApi.Account.AddSocket(_connector.ClientSocket.SpotApi);

            await Task.CompletedTask;
        }

        #region PublicApi

        public async Task<DateTime> GetServerTimestamp()
        {
            var timestamp = await _client.SpotApi.ExchangeData.GetServerTimeAsync();

            if (timestamp.Success)
            {
                return timestamp.Data.ServerTime;
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

            var data = await _client.SpotApi.ExchangeData.GetSpotSymbolsAsync();
            if (!data.Success)
            {
                return new SpotCoinModel { InstrumentName = instrumentName };
            }

            var instrument = data.Data.SymbolList.FirstOrDefault(x => string.Equals(x.Symbol, instrumentName, StringComparison.OrdinalIgnoreCase));

            if (instrument == null)
            {
                return new SpotCoinModel { InstrumentName = instrumentName };
            }

            var parts = instrument.Symbol.Split('_');
            var baseSymbol = parts[0].ToUpper();
            var quoteSymbol = parts[1].ToUpper();

            return new SpotCoinModel
            {
                InstrumentName = instrumentName,
                InstrumentType = InstrumentType.Spot,
                StockName = StockName,
                OriginalInstrumentName = instrumentName,
                MinMovementVolume = MinMovement(instrument.AmountPrecision),
                MinMovement = MinMovement(instrument.PricePrecision),
                MinVolumeInBaseCurrency = instrument.MinimumAmount,
                //MinVolumeInQuoteCurrency = instrument.MinimumValue,
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

            var data = await _client.SpotApi.ExchangeData.GetSpotSymbolsAsync();
            if (!data.Success)
            {
                throw new Exception(data.Error?.ToString());
            }

            return data.Data.SymbolList.Select(x =>
            {
                var parts = x.Symbol.Split('_');
                var baseSymbol = parts[0].ToUpper();
                var quoteSymbol = parts[1].ToUpper();

                return new SpotCoinModel
                {
                    InstrumentName = x.Symbol,
                    InstrumentType = InstrumentType.Spot,
                    StockName = StockName,
                    OriginalInstrumentName = x.Symbol,
                    MinMovementVolume = MinMovement(x.AmountPrecision),
                    MinMovement = MinMovement(x.PricePrecision),
                    MinVolumeInBaseCurrency = x.MinimumAmount,
                    //MinVolumeInQuoteCurrency = x.MinimumValue,
                    BaseCurrency = baseSymbol,
                    QuoteCurrency = quoteSymbol
                };
            }).ToList();
        }



        public async Task<decimal> Get24hVolume(string coinName, InstrumentType instrumentType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var ticker = await _client.SpotApi.ExchangeData.GetTickerAsync(coinName);

            return ticker is { Success: true, Data: not null } && ticker.Data.Ticker.Any()
                ? ticker.Data.Ticker.First().Volume
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

            var kLineInfo = await _client.SpotApi.ExchangeData.GetKlinesAsync(coinName, DigifinexKlineInterval.FiveMinutes, start, end);
            return kLineInfo is { Success: true, Data: not null }
                ? kLineInfo.Data.Data.Sum(e => e.Volume)
                : 0m;
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

            var data = await _client.SpotApi.ExchangeData.GetOrderBookAsync(symbol, depth);
            var result = data.Data;
            if (data.Success && result != null)
            {
                return new OrderBook
                {
                    InstrumentName = symbol,
                    InstrumentType = instrumentType,
                    OriginalInstrumentName = symbol,
                    StockName = StockName,
                    SystemTime = DateTime.UtcNow,
                    Asks = result.Asks
                        .OrderBy(x => x.Price)
                        .Select(x => new OrderBookEntry { Price = x.Price, Amount = x.Quantity })
                        .ToList(),
                    Bids = result.Bids
                        .OrderByDescending(x => x.Price)
                        .Select(x => new OrderBookEntry { Price = x.Price, Amount = x.Quantity })
                        .ToList(),
                };
            }

            return null;
        }

        public async Task<List<KlineItem>> GetKlines(string instrumentName, KlineInterval klineInterval, DateTime? fromDateUtc, DateTime? toDateUtc, int? limit = null, InstrumentType instrumentType = InstrumentType.Spot)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var data = await _client.SpotApi.ExchangeData.GetKlinesAsync(
                instrumentName,
                klineInterval.ToDigifinexKlineInterval(),
                fromDateUtc,
                toDateUtc);

            if (data is { Success: true, Data: not null })
            {
                return data.Data.Data.Select(x => new KlineItem
                {
                    ClosePrice = x.Close,
                    HighPrice = x.High,
                    LowPrice = x.Low,
                    OpenPrice = x.Open,
                    OpenTime = x.Timestamp,
                    Volume = x.Volume,
                }).ToList();
            }

            return [];
        }

        public async Task<OrderInfo> GetLastTrade(string instrumentName, InstrumentType instrumentType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var trades = await _client.SpotApi.ExchangeData.GetRecentTradesAsync(instrumentName, limit: 10);

            if (!trades.Success || !trades.Data.Data.Any())
            {
                return null;
            }

            var lastTrade = trades.Data.Data.First();
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
                Volume = lastTrade.Amount,
                OriginalInstrumentName = instrumentName,
                TransactionDate = lastTrade.Date,
                TradeId = lastTrade.Id.ToString()
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
                var orders = await _client.SpotApi.Account.GetCurrentOrdersAsync(DigifinexMarketType.Spot, instrumentName);
                if (orders.Success)
                {
                    return orders.Data.Select(x => new BaseOrderModel
                    {
                        Success = true,
                        InstrumentName = instrumentName,
                        OriginalInstrumentName = x.Symbol,
                        StockName = StockName,
                        Price = x.Price,
                        StockOrderId = x.OrderId,
                        InstrumentType = instrumentType,
                        Type = x.Type is DigifinexOrderType.Buy or DigifinexOrderType.Sell ? OrderType.Limit : OrderType.Market,
                        Volume = x.Amount,
                        TimeStamp = x.CreatedDate,
                        Direction = x.Type is DigifinexOrderType.Buy or DigifinexOrderType.BuyMarket ? Direction.BUY : Direction.SELL,
                        SystemOrderId = GetInternalOrderGuid(x.OrderId),
                        FilledVolume = x.ExecutedAmount

                    }).ToList();
                }
            }
            return null;
        }

        public async Task<OrderSyncInfo?> GetOrderInfo(string instrumentName, string orderId, InstrumentType instrumentType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var data = await _client.SpotApi.Account.GetOrderAsync(DigifinexMarketType.Spot, orderId);
            if (!data.Success || !data.Data.Any())
            {
                return null;
            }

            var order = data.Data.First();
            if (order != null)
            {
                return new OrderSyncInfo
                {
                    Price = order.Price,
                    FilledVolume = order.ExecutedAmount,
                    StockOrderId = orderId,
                    Volume = order.Amount,
                    InstrumentName = instrumentName,
                    Direction = order.Type is DigifinexOrderType.Buy or DigifinexOrderType.BuyMarket ? Direction.BUY : Direction.SELL,
                    DateCreated = order.CreatedDate,
                    DateUpdated = order.FinishedDate ?? order.CreatedDate,
                    Status = OrderHistoryEnumConvert(order.Status),
                    AvgPrice = order.AvgPrice ?? order.Price
                };
            }

            return null;
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

            var cancelOrder = await _client.SpotApi.Account.CancelOrderAsync(DigifinexMarketType.Spot, instrumentName, orderId);
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
                Error = $"{cancelOrder.Error?.Code} {cancelOrder.Error?.Message} {cancelOrder.Error?.Data}",
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

            var side = direction == Direction.BUY ? "buy" : "sell";
            string type;

            type = orderType == OrderType.Limit ? $"{side}" : $"{side}_market";
            

            if (orderType == OrderType.Market)
            {
                price = null;
            }

            var newOrder = await _client.SpotApi.Account.PlaceOrderAsync(
                market: DigifinexMarketType.Spot,
                symbol: instrument,
                type: type,
                amount: volume,
                price: price);

            if (newOrder.Success)
            {
                if (_orderInternalIdsDictionary.TryAdd(new string(newOrder.Data.OrderId.TakeLast(9).ToArray()), newOrder.Data.OrderId))
                {
                    _logger.LogTrace($"MakeOrderAsync added {newOrder.Data.OrderId} _ordersGuids count:{_orderInternalIdsDictionary.Count} {StockName} {instrument}");
                }
                else
                {
                    _logger.LogWarning($"MakeOrderAsync TryAdd false, OrderId:{newOrder.Data.OrderId} count:{_orderInternalIdsDictionary.Count} {StockName} {instrument}");
                }

                result.Success = true;
                result.Price = price.GetValueOrDefault();
                result.Volume = volume;
                result.StockOrderId = newOrder.Data.OrderId;
                result.TimeStamp = DateTime.UtcNow;
                result.SystemOrderId = GetInternalOrderGuid(newOrder.Data.OrderId);
            }
            else
            {
                result.ErrorType = BaseOrderModel.OrderErrorType.Unknown;
                result.Error = newOrder.Error?.ToString() ?? $"{StockName} error";
                if (newOrder.ResponseStatusCode.HasValue && (int)newOrder.ResponseStatusCode >= 500 && (int)newOrder.ResponseStatusCode < 600)
                {
                    result.ErrorType = BaseOrderModel.OrderErrorType.ServiceUnavailable;
                }

                if (result.Error.Contains("Too many visits") || result.Error.Contains("rate limit"))
                {
                    result.ErrorType = BaseOrderModel.OrderErrorType.ServiceUnavailable;
                }
            }

            return result;
        }

        public async Task<List<OrderHistoryItem>> GetOrderHistory(string instrumentName, DateTime fromDateUtc, DateTime toDateUtc, InstrumentType instrumentType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var result = new List<OrderHistoryItem>();
            var hasMoreData = true;
            var limit = 100;

            while (hasMoreData)
            {
                var data = await _client.SpotApi.Account.GetOrderHistoryAsync(
                    DigifinexMarketType.Spot,
                    instrumentName,
                    limit: limit,
                    startTime: ((DateTimeOffset)fromDateUtc).ToUnixTimeSeconds(),
                    endTime: ((DateTimeOffset)toDateUtc).ToUnixTimeSeconds());

                if (data.Success)
                {
                    result.AddRange(data.Data.Select(x => new OrderHistoryItem
                    {
                        StockOrderId = x.OrderId,
                        InstrumentName = x.Symbol,
                        DateCreated = x.CreatedDate,
                        DateUpdated = x.FinishedDate ?? x.CreatedDate,
                        Direction = x.Type is DigifinexOrderType.Buy or DigifinexOrderType.BuyMarket ? Direction.BUY : Direction.SELL,
                        FilledVolume = x.ExecutedAmount,
                        Volume = x.Amount,
                        Orderid = Guid.Empty,
                        Price = x.Price,
                        Status = OrderHistoryEnumConvert(x.Status),
                        AvgPrice = x.AvgPrice ?? x.Price
                    }).ToList());
                }

                if (data.Data.Count() < limit)
                {
                    hasMoreData = false;
                }
                else
                {
                    var oldestItemDate = data.Data.Min(e => e.CreatedDate);
                    toDateUtc = oldestItemDate.AddMilliseconds(-1);
                }
            }

            return result;
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

            if (balanceType == BalanceType.Spot)
            {
                var balance = await _client.SpotApi.Account.GetSpotAssetsAsync();
                if (!balance.Success)
                {
                    return new BalancesModel { StockName = StockName, Balances = [] };
                }

                var coinBalance = balance.Data.FirstOrDefault(x => x.Currency.Equals(coinName, StringComparison.OrdinalIgnoreCase));
                return new BalancesModel
                {
                    StockName = StockName,
                    Balances =
                    [
                        new Balance
                        {
                            Currency = coinName,
                            AvalibleBalance = coinBalance?.Free ?? 0,
                            FreezedBalance = (coinBalance?.Total - coinBalance?.Free) ?? 0,
                        }
                    ]
                };
            }
            else
            {
                var balance = await _client.SpotApi.Account.GetMarginAssetsAsync();
                if (!balance.Success)
                {
                    return new BalancesModel { StockName = StockName, Balances = [] };
                }

                var marginData = balance.Data as DigifinexMarginAssets;
                var coinBalance = marginData?.List.FirstOrDefault(x => x.Currency.Equals(coinName, StringComparison.OrdinalIgnoreCase));
                return new BalancesModel
                {
                    StockName = StockName,
                    Balances =
                    [
                        new Balance
                        {
                            Currency = coinName,
                            AvalibleBalance = coinBalance?.Free ?? 0,
                            FreezedBalance = coinBalance?.Total - coinBalance?.Free ?? 0,
                        }
                    ]
                };
            }
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

            if (balanceType == BalanceType.Futures)
            {
                throw new NotImplementedException("Retrieving Futures balances is not implemented.");
            }

            var accountInfo = await _client.SpotApi.Account.GetSpotAssetsAsync();
            if (!accountInfo.Success)
            {
                throw new Exception($"{accountInfo.OriginalData}");
            }

            var coinBalances = accountInfo.Data;

            return coinBalances.Where(e => e.Free != 0 || (e.Total - e.Free) != 0).Select(e => new AssetBalance
            {
                Currency = e.Currency,
                AvailableBalance = e.Free,
                FrozenBalance = e.Total - e.Free
            }).ToList();
        }

        public async Task<ExchangeResponse<List<MarketTickerModel>>> FetchTickersAsync()
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var data = await _client.SpotApi.ExchangeData.GetTickerAsync();
            if (!data.Success)
            {
                return ExchangeResponse<List<MarketTickerModel>>.Failed(data.OriginalData);
            }

            var result = data.Data.Ticker.Select(x =>
            {
                var parts = x.Symbol.Split('_');
                var baseAsset = parts[0].ToUpper();
                var quoteAsset = parts[1].ToUpper();

                return new MarketTickerModel
                {
                    Symbol = x.Symbol,
                    BaseAsset = baseAsset,
                    QuoteAsset = quoteAsset,
                    High = x.High,
                    Low = x.Low,
                    Last = x.Last,
                    Change = x.Change,
                    BaseVolume = x.BaseVolume,
                    QuoteVolume = x.Volume
                };
            });

            return ExchangeResponse<List<MarketTickerModel>>.Success(result.Where(x => x != null).ToList()!, data.OriginalData);
        }

        public Task<ExchangeResponse<OptionTickerModel>> GetOptionTicker(string instrumentName)
        {
            throw new NotImplementedException();
        }

        public async Task<List<OrderHistoryItem>> GetOrderHistoryForPeriodWithFee(string instrumentName, DateTime fromDateUtc, DateTime toDateUtc, InstrumentType instrumentType)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            List<Digifinex.Net.Objects.Models.Account.DigifinexTrade> trades = [];
            var hasMoreData = true;
            const int limit = 100;

            while (hasMoreData)
            {
                var data = await _client.SpotApi.Account.GetMyTradesAsync(
                    DigifinexMarketType.Spot,
                    instrumentName,
                    limit: limit,
                    startTime: ((DateTimeOffset)fromDateUtc).ToUnixTimeSeconds(),
                    endTime: ((DateTimeOffset)toDateUtc).ToUnixTimeSeconds());

                if (data.Success)
                {
                    trades.AddRange(data.Data.ToList());
                }

                if (data.Data.Count() < limit)
                {
                    hasMoreData = false;
                }
                else
                {
                    var oldestItemDate = data.Data.Min(e => e.Timestamp);
                    toDateUtc = DateTimeOffset.FromUnixTimeSeconds(oldestItemDate).DateTime.AddMilliseconds(-1);
                }
            }

            var result = trades.GroupBy(e => e.OrderId).Select(e =>
            new OrderHistoryItem
            {
                StockOrderId = e.First().OrderId,
                AvgPrice = e.Sum(z => z.Price * z.Amount) / e.Sum(z => z.Amount),
                Fee = e.Sum(z => z.Fee),
                FeeCurrency = e.First().FeeCurrency,
                DateUpdated = DateTimeOffset.FromUnixTimeSeconds(e.Last().Timestamp).DateTime,
                Price = e.Average(z => z.Price),
            }).ToList();

            return result;
        }

        #endregion PrivateApi

        #region deposit/withdraw/transfers

        public async Task<ExchangeResponse<List<DepositItemModel>>> GetDepositHistory(string asset,
            DateTime? dateFromUtc = null, int? limit = null)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var data = await _client.SpotApi.Account.GetDepositHistoryAsync(asset, from: 0, size: limit ?? 100);
            if (!data.Success)
            {
                return ExchangeResponse<List<DepositItemModel>>.Failed(data.OriginalData);
            }

            var result = data.Data
                .Select(x => new DepositItemModel()
                {
                    Id = x.Id.ToString(),
                    Txid = x.HashId,
                    Timestamp = x.CreatedDate.ToUnixTimeMilliseconds(),
                    DateTime = x.CreatedDate,
                    Address = x.Address ?? "",
                    Type = TransactionType.Deposit,
                    Amount = x.Amount,
                    Currency = asset,
                    Status = x.State.ToDepositStatus(),
                    Fee = null
                });

            return ExchangeResponse<List<DepositItemModel>>.Success(result.ToList(), data.OriginalData);
        }

        public async Task<ExchangeResponse<List<WithdrawalItemModel>>> GetWithdrawHistory(string asset,
            DateTime? dateFromUtc = null, int? limit = null)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var data = await _client.SpotApi.Account.GetWithdrawHistoryAsync(asset, from: "0", size: limit?.ToString() ?? "100");
            if (!data.Success)
            {
                return ExchangeResponse<List<WithdrawalItemModel>>.Failed(data.OriginalData);
            }

            var result = data.Data
                .Select(x => new WithdrawalItemModel()
                {
                    Id = x.Id.ToString(),
                    Txid = x.HashId,
                    Timestamp = x.CreatedDate.ToUnixTimeMilliseconds(),
                    DateTime = x.CreatedDate,
                    Address = x.Address ?? "",
                    Type = TransactionType.Withdrawal,
                    Amount = x.Amount,
                    Currency = asset,
                    Status = x.State.ToWithdrawStatus(),
                    Fee = new FeeModel()
                    {
                        Cost = x.Fee,
                        Currency = x.Currency
                    }
                });

            return ExchangeResponse<List<WithdrawalItemModel>>.Success(result.ToList(), data.OriginalData);
        }

        public async Task<ExchangeResponse<List<TransferItemModel>>> GetTransferHistory(string asset, int limit,
            DateTime? dateFromUtc = null)
        {
            if (_client == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            var data = await _client.SpotApi.Account.GetFinanceLogAsync(
                DigifinexMarketType.Spot, 
                asset, 
                startTime: dateFromUtc.HasValue ? ((DateTimeOffset)dateFromUtc.Value).ToUnixTimeSeconds() : null,
                limit: limit);

            if (!data.Success)
            {
                return ExchangeResponse<List<TransferItemModel>>.Failed(data.OriginalData);
            }

            var result = data.Data.Finance
                .Where(x => x.Type == 14 || x.Type == 15) // 14 = transfer in, 15 = transfer out
                .Select(x => new TransferItemModel()
                {
                    Timestamp = x.Time,
                    DateTime = DateTimeOffset.FromUnixTimeMilliseconds(x.Time).DateTime,
                    FromAccount = x.Type == 15 ? "spot" : "margin",
                    ToAccount = x.Type == 14 ? "spot" : "margin",
                    Currency = asset,
                    Status = TransferStatus.Ok
                });

            return ExchangeResponse<List<TransferItemModel>>.Success(result.ToList(), data.OriginalData);
        }

        #endregion

        #region NotImportant

        public Task<List<BaseInstrumentModel>> GetInstrumetsAsync(InstrumentType instrumentType)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseOrderModel> ClosePositionAsync(string instrumentName, string orderId)
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

        public async Task<Withdraw> Withdrawal(string instrumentName, decimal amount, string address, string memo, string uniqueid, string network = "")
        {
            throw new NotImplementedException();
        }

        public async Task<WithdrawHistory> WithdrawalHistory(string id)
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
            // Digifinex doesn't use listen keys for websocket authentication
            return new SocketListenKey { ListenKey = string.Empty };
        }

        public async Task<SocketKeepAliveListenKey> KeepAlive(InstrumentType instrumentType, string listenKey)
        {
            // Digifinex doesn't use listen keys for websocket authentication
            return new SocketKeepAliveListenKey();
        }

        private OrderHistoryItemStatus OrderHistoryEnumConvert(DigifinexOrderStatus status)
        {
            switch (status)
            {
                case DigifinexOrderStatus.NoneExecuted:
                    return OrderHistoryItemStatus.Open;
                    break;
                case DigifinexOrderStatus.PartiallyExecuted:
                    return OrderHistoryItemStatus.PartiallyFilled;
                    break;
                case DigifinexOrderStatus.FullyExecuted:
                    return OrderHistoryItemStatus.Filled;
                    break;
                case DigifinexOrderStatus.CancelledNoneExecuted:
                    return OrderHistoryItemStatus.Canceled;
                    break;
                case DigifinexOrderStatus.CancelledPartiallyExecuted:
                    return OrderHistoryItemStatus.Canceled;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public async Task DisconnectAsync()
        {
            await Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
            _client.Dispose();
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

        private Guid GetInternalOrderGuid(string externalOrderId)
        {
            var key = new string(externalOrderId.TakeLast(9).ToArray());

            if (_orderInternalIdsDictionary.TryGetValue(key, out var internalId))
            {
                return Guid.Parse(internalId);
            }
            
            return Guid.Empty;
        }
    }
}