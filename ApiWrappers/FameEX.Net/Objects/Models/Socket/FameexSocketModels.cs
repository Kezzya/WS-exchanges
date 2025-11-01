using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FameEX.Net.Objects.Models.Socket
{
    public class FameexSocketRequest
    {
        [JsonProperty("event")]
        public string Event { get; set; } = "sub";
        [JsonProperty("topic")]
        public string Topic { get; set; } = string.Empty;

        [JsonProperty("params")]
        public FameexSocketRequestParams Params { get; set; } = new FameexSocketRequestParams();
    }

    public class FameexSocketRequestParams
    {
        [JsonProperty("channel")]
        public string Channel { get; set; } = string.Empty;
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty; // Добавлено поле Symbol

        [JsonProperty("cb_id")]
        public string CallbackId { get; set; } = "1";
    }

    public class FameexSocketResponse<T>
    {
        [JsonProperty("channel")]
        public string Channel { get; set; } = string.Empty;

        [JsonProperty("ts")]
        public long Timestamp { get; set; }

        [JsonProperty("tick")]
        public T Tick { get; set; } = default!;
    }

    public class FameexSubscriptionResult
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("cb_id")]
        public string CallbackId { get; set; } = string.Empty;
    }

    public class FameexTrade
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; } // "buy" or "sell"

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("vol")]
        public decimal Volume { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("ts")]
        public long Timestamp { get; set; }

        [JsonProperty("ds")]
        public string DateString { get; set; }
    }
    public class FameexTicker
    {
        [JsonProperty("amount")]
        public decimal? Amount { get; set; }

        [JsonProperty("vol")]
        public decimal? Volume { get; set; }

        [JsonProperty("open")]
        public decimal? Open { get; set; }

        [JsonProperty("close")]
        public decimal? Close { get; set; }

        [JsonProperty("high")]
        public decimal? High { get; set; }

        [JsonProperty("low")]
        public decimal? Low { get; set; }

        [JsonProperty("rose")]
        public decimal? Rose { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
    }
    public enum FameexTradingMode
    {
        Spot = 1,
        Margin = 2
    }

    public class FameexTradesUpdate
    {
        public long Id { get; set; }
        public long Timestamp { get; set; }
        public List<FameexTrade> Data { get; set; } = new List<FameexTrade>();
    }

    public class FameexDepthUpdate
    {
        public List<FameexOrderBookEntry> Asks { get; set; } = new List<FameexOrderBookEntry>();
        public List<FameexOrderBookEntry> Bids { get; set; } = new List<FameexOrderBookEntry>();
    }

    public class FameexPong
    {
        [JsonProperty("result")]
        public string Result { get; set; } = string.Empty;
    }

    public class FameexOrder
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("filled")]
        public decimal Filled { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("mode")]
        public FameexTradingMode Mode { get; set; }

        [JsonProperty("notional")]
        public decimal Notional { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("price_avg")]
        public decimal PriceAvg { get; set; }

        [JsonProperty("side")]
        public FameexOrderSide Side { get; set; }

        [JsonProperty("status")]
        public FameexOrderStatus Status { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("ts")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }

        [JsonProperty("type")]
        public FameexOrderType Type { get; set; }
    }

    public enum FameexOrderType
    {
        Limit,
        Market
    }

    public enum FameexOrderStatus
    {
        Unfilled = 0,
        PartiallyFilled = 1,
        FullyFilled = 2,
        CanceledUnfilled = 3,
        CanceledPartiallyFilled = 4
    }

    public enum FameexOrderSide
    {
        Buy,
        Sell
    }

    public class FameexBalance
    {
        [JsonProperty("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonProperty("free")]
        public decimal Free { get; set; }

        [JsonProperty("total")]
        public decimal Total { get; set; }

        [JsonProperty("used")]
        public decimal Used { get; set; }
    }
    public class FameexOrderUpdate
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("clientOid")]
        public string ClientOrderId { get; set; }

        [JsonProperty("side")]
        public int Side { get; set; } // 1-buy, 2-sell

        [JsonProperty("orderType")]
        public int OrderType { get; set; } // 1-Limit, 2-Market

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("filledAmount")]
        public decimal FilledAmount { get; set; }

        [JsonProperty("filledMoney")]
        public decimal FilledMoney { get; set; }

        [JsonProperty("state")]
        public int State { get; set; } // 1-Created, 2-Waiting, 3-PartiallyFilled, 4-Filled, 5-PartiallyCancelled, 6-Cancelled

        [JsonProperty("createTime")]
        public long CreateTime { get; set; }

        [JsonProperty("updateTime")]
        public long UpdateTime { get; set; }
    }
    
    public class FameexServerTime
    {
        [JsonProperty("ts")]
        public long Timestamp { get; set; }
    }
    public class FameexOrderBook
    {
        [JsonProperty("asks")]
        public List<FameexOrderBookEntry> Asks { get; set; } = new();

        [JsonProperty("bids")]
        public List<FameexOrderBookEntry> Bids { get; set; } = new();
    }
    public class FameexOrderBookEntry
    {
        public decimal Price { get; set; }

        public decimal Quantity { get; set; }
    }

    public class FameexAuthResult
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class FameexAlgoOrder
    {
        [JsonProperty("algo_price")]
        public decimal AlgoPrice { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("mode")]
        public FameexTradingMode Mode { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; } = string.Empty;

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("ts")]
        public long Timestamp { get; set; }

        [JsonProperty("trigger_price")]
        public decimal TriggerPrice { get; set; }
    }
}