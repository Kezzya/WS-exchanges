using BaseStockConnector.Models.Enums;
using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Converters.JsonNet;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.CommonClients;
using CryptoExchange.Net.Objects;
using Fameex.Net.Objects.Models.ExchangeData;
using FameEX.Net.Objects.Models.Socket;
using FameEX.Net.Objects.Options;
using Microsoft.Extensions.Logging;
using System.Globalization;
using FameexOrderStatus = FameEX.Net.Objects.Models.Account.FameexOrderStatus;
using FameexOrderType = FameEX.Net.Objects.Models.Account.FameexOrderType;

namespace FameEX.Net.Clients.SpotApi
{
    public class FameexRestClientSpotApi : RestApiClient, ISpotClient
    {
        internal static TimeSyncState _timeSyncState = new TimeSyncState("Spot Api");

        public string ExchangeName => "Fameex";

        public event Action<OrderId>? OnOrderPlaced;
        public event Action<OrderId>? OnOrderCanceled;

        public FameexRestClientSpotApiExchangeData ExchangeData { get; }
        public FameexRestClientSpotApiAccount Account { get; }

        public FameexRestClientSpotApi(ILogger logger, HttpClient? httpClient, FameexRestOptions options)
            : base(logger, httpClient, options.Environment.RestClientAddress, options, options.SpotOptions)
        {
            ExchangeData = new FameexRestClientSpotApiExchangeData(logger, this);
            Account = new FameexRestClientSpotApiAccount(logger, this);
            RequestBodyFormat = RequestBodyFormat.FormData;
        }

        protected override IMessageSerializer CreateSerializer() => new JsonNetMessageSerializer();

        protected override IStreamMessageAccessor CreateAccessor() => new JsonNetStreamMessageAccessor();

        public override TimeSyncInfo? GetTimeSyncInfo() =>
            new TimeSyncInfo(_logger,
                (ApiOptions.AutoTimestamp ?? ClientOptions.AutoTimestamp),
                (ApiOptions.TimestampRecalculationInterval ?? ClientOptions.TimestampRecalculationInterval),
                _timeSyncState);

        public override TimeSpan? GetTimeOffset() => _timeSyncState.TimeOffset;

        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
            => new FameexAuthenticationProvider(credentials);

        internal Uri GetUri(string path) => new Uri(BaseAddress.AppendPath(path));

        internal Task<WebCallResult> SendAsync(RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null)
            => SendToAddressAsync(BaseAddress, definition, parameters, cancellationToken, weight);

        internal async Task<WebCallResult> SendToAddressAsync(string baseAddress, RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null)
        {
            return await base.SendAsync(baseAddress, definition, parameters, cancellationToken, null, weight).ConfigureAwait(false);
        }

        internal Task<WebCallResult<T>> SendAsync<T>(RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null, Dictionary<string, string>? additionalHeaders = null) where T : class
            => SendToAddressAsync<T>(BaseAddress, definition, parameters, cancellationToken, weight, additionalHeaders);

        internal async Task<WebCallResult<T>> SendToAddressAsync<T>(string baseAddress, RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null, Dictionary<string, string>? additionalHeaders = null) where T : class
        {
            var result = await base.SendAsync<T>(baseAddress, definition, parameters, cancellationToken, additionalHeaders, weight).ConfigureAwait(false);
            return result;
        }

        protected override ServerRateLimitError ParseRateLimitResponse(int httpStatusCode, IEnumerable<KeyValuePair<string, IEnumerable<string>>> responseHeaders, IMessageAccessor accessor)
        {
            var retryAfterHeader = responseHeaders.SingleOrDefault(r => r.Key.Equals("Retry-After", StringComparison.InvariantCultureIgnoreCase));
            if (retryAfterHeader.Value?.Any() == true && int.TryParse(retryAfterHeader.Value.First(), out var seconds))
                return new ServerRateLimitError("Rate limit exceeded") { RetryAfter = DateTime.UtcNow.AddSeconds(seconds) };

            return new ServerRateLimitError("Rate limit exceeded") { RetryAfter = DateTime.UtcNow.AddSeconds(10) };
        }

        protected override ServerError? TryParseError(IMessageAccessor accessor)
        {
            var code = accessor.GetValue<int?>(MessagePath.Get().Property("code"));
            var msg = accessor.GetValue<string>(MessagePath.Get().Property("message"));

            if (code == null || code == 200 || code == 0)
                return null;

            return new ServerError(code.Value, msg ?? "Unknown error");
        }

        public override string FormatSymbol(string baseAsset, string quoteAsset) =>
            $"{baseAsset.ToUpperInvariant()}_{quoteAsset.ToUpperInvariant()}";

        public string GetSymbolName(string baseAsset, string quoteAsset) => FormatSymbol(baseAsset, quoteAsset);

        #region ISpotClient Implementation

        public async Task<WebCallResult<IEnumerable<Symbol>>> GetSymbolsAsync(CancellationToken ct = default)
        {
            var result = await ExchangeData.GetMarketsAsync(ct).ConfigureAwait(false);

            if (!result.Success || result.Data?.Symbols == null)
                return result.AsError<IEnumerable<Symbol>>(result.Error!);

            var symbols = result.Data.Symbols.Select(m => new Symbol
            {
                SourceObject = m,
                Name = m.Symbol.ToUpper(),
                MinTradeQuantity = m.LimitVolumeMin,
                QuantityDecimals = m.QuantityPrecision,
                PriceDecimals = m.PricePrecision
            });

            return result.As<IEnumerable<Symbol>>(symbols);
        }
        public async Task<WebCallResult<Ticker>> GetTickerAsync(string symbol, CancellationToken ct = default)
        {
            var result = await ExchangeData.GetTickerAsync(symbol, ct).ConfigureAwait(false);
            if (!result.Success || result.Data == null)
                return result.AsError<Ticker>(result.Error!);

            var ticker = new Ticker
            {
                SourceObject = result.Data,
                Symbol = symbol, 
                LastPrice = result.Data.LastPrice, 
                HighPrice = result.Data.High,      
                LowPrice = result.Data.Low,        
                Volume = result.Data.Volume,      
                Price24H = ParseChangePercentage(result.Data.Change24H) // rose -> Change24H и нужно парсить строку
            };

            return result.As(ticker);
        }


        //public async Task<WebCallResult<IEnumerable<Ticker>>> GetTickersAsync(CancellationToken ct = default)
        //{
        //    var result = await ExchangeData.GetTickersAsync(ct).ConfigureAwait(false);
        //    if (!result.Success || result.Data == null)
        //        return result.AsError<IEnumerable<Ticker>>(result.Error!);

        //    var tickers = result.Data.Select(t => new Ticker
        //    {
        //        SourceObject = t,
        //        Symbol =  string.Empty,
        //        LastPrice = t.LastPrice,
        //        HighPrice = t.High,
        //        LowPrice = t.Low,
        //        Volume = t.Volume,
        //        Price24H = ParseChangePercentage(t.Change24H)
        //    });

        //    return result.As<IEnumerable<Ticker>>(tickers);
        //}
        private static decimal ParseChangePercentage(string rose)
        {
            if (string.IsNullOrWhiteSpace(rose))
                return 0;

            // Убираем знак %, если есть (в документации его нет, но на всякий случай)
            var cleaned = rose.Replace("%", "");

            // Проверяем, есть ли знак
            if (decimal.TryParse(cleaned, out var value))
                return value;

            return 0;
        }
        public async Task<WebCallResult<IEnumerable<Kline>>> GetKlinesAsync(
            string symbol,
            TimeSpan timespan,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int? limit = null,
            CancellationToken ct = default)
        {
            var interval = ConvertTimeSpanToInterval(timespan);

            var result = await ExchangeData.GetKlinesAsync(symbol, interval, startTime, endTime, limit, ct).ConfigureAwait(false);
            if (!result.Success || result.Data == null)
                return result.AsError<IEnumerable<Kline>>(result.Error!);

            var klines = result.Data.Select(k => new Kline
            {
                SourceObject = k,
                OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(k.Time).UtcDateTime,
                OpenPrice = k.Open,
                HighPrice = k.High,
                LowPrice = k.Low,
                ClosePrice = k.Close,
                Volume = k.Volume
            });

            return result.As<IEnumerable<Kline>>(klines);
        }

        public async Task<WebCallResult<OrderBook>> GetOrderBookAsync(string symbol, CancellationToken ct = default)
        {
            var result = await ExchangeData.GetOrderBookAsync(symbol, ct).ConfigureAwait(false);
            if (!result.Success || result.Data == null)
                return result.AsError<OrderBook>(result.Error!);

            var orderBook = new OrderBook
            {
                SourceObject = result.Data,
                Asks = result.Data.Asks?.Select(a => new OrderBookEntry
                {
                    Price = a.Price,
                    Quantity = a.Quantity
                }) ?? Enumerable.Empty<OrderBookEntry>(),
                Bids = result.Data.Bids?.Select(b => new OrderBookEntry
                {
                    Price = b.Price,
                    Quantity = b.Quantity
                }) ?? Enumerable.Empty<OrderBookEntry>()
            };

            return result.As(orderBook);
        }

        public async Task<WebCallResult<IEnumerable<Trade>>> GetRecentTradesAsync(string symbol, CancellationToken ct = default)
        {
            var result = await ExchangeData.GetRecentTradesAsync(symbol, ct).ConfigureAwait(false);
            if (!result.Success || result.Data == null)
                return result.AsError<IEnumerable<Trade>>(result.Error!);

            var trades = result.Data.Select(t => new Trade
            {
                SourceObject = t,
                Symbol = symbol,
                Price = t.Price,
                Quantity = t.Quantity,
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(t.Time).UtcDateTime,
            });

            return result.As<IEnumerable<Trade>>(trades);
        }

        public async Task<WebCallResult<IEnumerable<Balance>>> GetBalancesAsync(string? accountId = null, CancellationToken ct = default)
        {
            var accountInfo = await Account.GetAccountInfoAsync(ct).ConfigureAwait(false);
            if (!accountInfo.Success || accountInfo.Data?.Balances == null)
                return accountInfo.AsError<IEnumerable<Balance>>(accountInfo.Error!);

            var balances = accountInfo.Data.Balances.Select(b =>
            {
                decimal.TryParse(b.Free, NumberStyles.Any, CultureInfo.InvariantCulture, out var free);
                decimal.TryParse(b.Locked, NumberStyles.Any, CultureInfo.InvariantCulture, out var locked);

                return new Balance
                {
                    SourceObject = b,
                    Asset = b.Asset,
                    Total = free + locked,
                    Available = free
                };
            });

            return accountInfo.As<IEnumerable<Balance>>(balances);
        }


        public async Task<WebCallResult<Order>> GetOrderAsync(string orderId, string? symbol = null, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(symbol))
                return new WebCallResult<Order>(new ArgumentError("Symbol is required"));

            var result = await Account.GetOrderAsync(symbol, orderId, ct).ConfigureAwait(false);
            if (!result.Success || result.Data == null)
                return result.AsError<Order>(result.Error!);

            var order = new Order
            {
                SourceObject = result.Data,
                Symbol = result.Data.Symbol,
                Id = result.Data.OrderId,
                Price = result.Data.Price,
                Quantity = result.Data.Amount,
                QuantityFilled = result.Data.FilledAmount,
                Status = (CommonOrderStatus)ConvertOrderStatus(result.Data.Status),
                Side = sideFromOrderType(result.Data.Type), // Исправлено: используем новый метод
                Type = (CommonOrderType)ConvertOrderType(result.Data.Type),
                Timestamp = result.Data.CreatedDate
            };

            return result.As(order);
        }

        // Добавьте этот вспомогательный метод в #region Helper Methods
        private CommonOrderSide sideFromOrderType(FameexOrderType type)
        {
            // Предполагаем, что Buy соответствует Limit, Sell соответствует Market
            // Если есть более точная информация, скорректируйте логику
            return type == FameexOrderType.Limit ? CommonOrderSide.Buy : CommonOrderSide.Sell;
        }

        //public async Task<WebCallResult<IEnumerable<UserTrade>>> GetOrderTradesAsync(string orderId, string? symbol = null, CancellationToken ct = default)
        //{
        //    if (string.IsNullOrEmpty(symbol))
        //        return new WebCallResult<IEnumerable<UserTrade>>(new ArgumentError("Symbol is required"));

        //    var result = await Account.GetTradeHistoryAsync(symbol, orderId, ct: ct).ConfigureAwait(false);
        //    if (!result.Success || result.Data == null)
        //        return result.AsError<IEnumerable<UserTrade>>(result.Error!);

        //    var trades = result.Data.Select(t => new UserTrade
        //    {
        //        SourceObject = t,
        //        Symbol = t.Symbol,
        //        OrderId = t.OrderId,
        //        Id = t.Id,
        //        Price = t.Price,
        //        Quantity = t.Amount,
        //        Fee = t.Fee,
        //        FeeAsset = t.FeeCurrency,
        //        Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(t.Timestamp).DateTime,
        //        //Direction = t.Side.Equals("buy", StringComparison.OrdinalIgnoreCase) ? Direction.BUY : Direction.SELL
        //    });

        //    return result.As<IEnumerable<UserTrade>>(trades);
        //}

        public async Task<WebCallResult<IEnumerable<Order>>> GetOpenOrdersAsync(string? symbol = null, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(symbol))
                return new WebCallResult<IEnumerable<Order>>(new ArgumentError("Symbol is required"));

            var result = await Account.GetOpenOrdersAsync(symbol, ct).ConfigureAwait(false);
            if (!result.Success || result.Data == null)
                return result.AsError<IEnumerable<Order>>(result.Error!);

            // Замените строку:
            // Side = ConvertOrderSide(o.Type),
            // на:
            // Side = sideFromOrderType(o.Type),

            // Итоговый фрагмент:
            var orders = result.Data.Select(o => new Order
            {
                SourceObject = o,
                Symbol = o.Symbol,
                Id = o.OrderId,
                Price = o.Price,
                Quantity = o.Amount,
                QuantityFilled = o.FilledAmount,
                Status = (CommonOrderStatus)ConvertOrderStatus(o.Status),
                Side = sideFromOrderType(o.Type),
                Type = (CommonOrderType)ConvertOrderType(o.Type),
                Timestamp = o.CreatedDate
            });

            return result.As<IEnumerable<Order>>(orders);
        }

        //public async Task<WebCallResult<IEnumerable<Order>>> GetClosedOrdersAsync(string? symbol = null, CancellationToken ct = default)
        //{
        //    if (string.IsNullOrEmpty(symbol))
        //        return new WebCallResult<IEnumerable<Order>>(new ArgumentError("Symbol is required"));

        //    var result = await Account.GetOrderHistoryAsync(symbol, FameexOrderStatus.Filled, ct: ct).ConfigureAwait(false);
        //    if (!result.Success || result.Data == null)
        //        return result.AsError<IEnumerable<Order>>(result.Error!);

        //    var orders = result.Data.Select(o => new Order
        //    {
        //        SourceObject = o,
        //        Symbol = o.Symbol,
        //        Id = o.OrderId,
        //        Price = o.Price,
        //        Quantity = o.Amount,
        //        QuantityFilled = o.FilledAmount,
        //        Status = (CommonOrderStatus)OrderStatus.Filled,
        //        Side = sideFromOrderType(o.Type), // Исправлено: используем правильный метод
        //        Type = (CommonOrderType)ConvertOrderType(o.Type),
        //        Timestamp = o.CreatedDate
        //    });

        //    return result.As<IEnumerable<Order>>(orders);
        //}

        //public async Task<WebCallResult<OrderId>> PlaceOrderAsync(
        //    string symbol,
        //    CommonOrderSide side,
        //    CommonOrderType type,
        //    decimal quantity,
        //    decimal? price = null,
        //    string? accountId = null,
        //    string? clientOrderId = null,
        //    CancellationToken ct = default)
        //{
        //    var fameexSide = side == CommonOrderSide.Buy ? FameexOrderSide.Buy : FameexOrderSide.Sell;
        //    var fameexType = type == CommonOrderType.Market ? FameexOrderType.Market : FameexOrderType.Limit;

        //    var result = await Account.PlaceOrderAsync(symbol, fameexType, quantity, price, clientOrderId, ct).ConfigureAwait(false);
        //    if (!result.Success || result.Data?.OrderId == null)
        //        return result.AsError<OrderId>(result.Error!);

        //    var orderId = new OrderId
        //    {
        //        SourceObject = result.Data,
        //        Id = result.Data.OrderId
        //    };

        //    OnOrderPlaced?.Invoke(orderId);

        //    return result.As(orderId);
        //}

        //public async Task<WebCallResult<OrderId>> CancelOrderAsync(string orderId, string? symbol = null, CancellationToken ct = default)
        //{
        //    if (string.IsNullOrEmpty(symbol))
        //        return new WebCallResult<OrderId>(new ArgumentError("Symbol is required"));

        //    var result = await Account.CancelOrderAsync(symbol, orderId, ct).ConfigureAwait(false);
        //    if (!result.Success || result.Data == null)
        //        return result.AsError<OrderId>(result.Error!);

        //    if (result.Data.Success?.Contains(orderId) == true)
        //    {
        //        var orderIdObj = new OrderId
        //        {
        //            SourceObject = orderId,
        //            Id = orderId
        //        };

        //        OnOrderCanceled?.Invoke(orderIdObj);

        //        return result.As(orderIdObj);
        //    }

        //    return new WebCallResult<OrderId>(new ServerError("Order not cancelled"));
        //}

        #endregion

        #region Helper Methods

        private FameexKlineInterval ConvertTimeSpanToInterval(TimeSpan timespan)
        {
            return timespan switch
            {
                { TotalMinutes: 1 } => FameexKlineInterval.OneMinute,
                { TotalMinutes: 5 } => FameexKlineInterval.FiveMinutes,
                { TotalMinutes: 15 } => FameexKlineInterval.FifteenMinutes,
                { TotalMinutes: 30 } => FameexKlineInterval.ThirtyMinutes,
                { TotalHours: 1 } => FameexKlineInterval.OneHour,
                { TotalHours: 4 } => FameexKlineInterval.FourHours,
                { TotalHours: 12 } => FameexKlineInterval.TwelveHours,
                { TotalDays: 1 } => FameexKlineInterval.OneDay,
                { TotalDays: 7 } => FameexKlineInterval.OneWeek,
                _ => throw new ArgumentException($"Unsupported timespan: {timespan}")
            };
        }

        private OrderStatus ConvertOrderStatus(FameexOrderStatus status)
        {
            return status switch
            {
                FameexOrderStatus.NewOrder
                or FameexOrderStatus.New
                or FameexOrderStatus.NewUnderscore => OrderStatus.New,

                FameexOrderStatus.PartiallyFilled => OrderStatus.PartiallyFilled,

                FameexOrderStatus.Filled => OrderStatus.Filled,

                FameexOrderStatus.Canceled
                or FameexOrderStatus.PendingCancel
                or FameexOrderStatus.Rejected
                or FameexOrderStatus.Expired => OrderStatus.Canceled,

                FameexOrderStatus.Insurance => OrderStatus.Insurance,

                FameexOrderStatus.Adl => OrderStatus.Adl,

                _ => OrderStatus.Canceled // По умолчанию — отменён (безопасно)
            };
        }

        private FameexOrderSide ConvertOrderSide(FameexOrderSide side)
        {
            return (FameexOrderSide)(side == FameexOrderSide.Buy
                ? Direction.BUY
                : Direction.SELL);
        }

        private FameexOrderType ConvertOrderType(FameexOrderType type)
        {
            return (FameexOrderType)(type == FameexOrderType.Market
                ? OrderType.Market
                : OrderType.Limit);
        }

        public Task<WebCallResult<IEnumerable<Ticker>>> GetTickersAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<WebCallResult<OrderId>> PlaceOrderAsync(
            string symbol,
            CommonOrderSide side,
            CommonOrderType type,
            decimal quantity,
            decimal? price = null,
            string? accountId = null,
            string? clientOrderId = null,
            CancellationToken ct = default)
        {
            var fameexType = side == CommonOrderSide.Buy
                ? (type == CommonOrderType.Market ? FameexOrderType.Market : FameexOrderType.Market)
                : (type == CommonOrderType.Market ? FameexOrderType.Limit : FameexOrderType.Limit);

            var result = await Account.PlaceOrderAsync(symbol, fameexType, quantity, price, clientOrderId, ct).ConfigureAwait(false);
            if (!result.Success || result.Data?.OrderId == null)
                return result.AsError<OrderId>(result.Error!);

            var orderId = new OrderId
            {
                SourceObject = result.Data,
                Id = result.Data.OrderId
            };

            OnOrderPlaced?.Invoke(orderId);

            return result.As(orderId);
        }

        public Task<WebCallResult<IEnumerable<UserTrade>>> GetOrderTradesAsync(string orderId, string? symbol = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebCallResult<IEnumerable<Order>>> GetClosedOrdersAsync(string? symbol = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebCallResult<OrderId>> CancelOrderAsync(string orderId, string? symbol = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}