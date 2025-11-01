using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace FameEX.Net.Models
{
    // Symbol Info
    public class FameexSymbol
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("baseAsset")]
        public string BaseAsset { get; set; } = string.Empty;

        [JsonProperty("quoteAsset")]
        public string QuoteAsset { get; set; } = string.Empty;

        [JsonProperty("pricePrecision")]
        public int PricePrecision { get; set; }

        [JsonProperty("quantityPrecision")]
        public int QuantityPrecision { get; set; }

        [JsonProperty("limitVolumeMin")]
        public decimal LimitVolumeMin { get; set; }

        [JsonProperty("marketBuyMin")]
        public decimal MarketBuyMin { get; set; }

        [JsonProperty("marketSellMin")]
        public decimal MarketSellMin { get; set; }

        [JsonProperty("limitPriceMin")]
        public decimal LimitPriceMin { get; set; }
    }

    public class FameexSymbolsResponse
    {
        [JsonProperty("symbols")]
        public List<FameexSymbol> Symbols { get; set; } = new();
    }

    // Server Time
    public class FameexServerTime
    {
        [JsonProperty("timezone")]
        public string Timezone { get; set; } = string.Empty;

        [JsonProperty("serverTime")]
        public long ServerTime { get; set; }
    }

    // Ticker
    public class FameexTicker
    {
        [JsonProperty("symbol")]
        public string? Symbol { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("high")]
        public decimal High { get; set; }

        [JsonProperty("low")]
        public decimal Low { get; set; }

        [JsonProperty("last")]
        public decimal Last { get; set; }

        [JsonProperty("vol")]
        public decimal Volume { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("buy")]
        public decimal Bid { get; set; }

        [JsonProperty("sell")]
        public decimal Ask { get; set; }

        [JsonProperty("rose")]
        public string Rose { get; set; } = string.Empty;
    }

    // Trade
    public class FameexTrade
    {
        [JsonProperty("side")]
        public string Side { get; set; } = string.Empty;

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("qty")]
        public decimal Quantity { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }
    }

    // Kline
    public class FameexKline
    {
        public long Time { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
    }

    // Order
    public class FameexOrder
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("volume")]
        public decimal Volume { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("filledVolume")]
        public decimal FilledVolume { get; set; }

        [JsonProperty("filledAmount")]
        public decimal FilledAmount { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }
    }

    // Place Order Request
    public class FameexPlaceOrderRequest
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("side")]
        public string Side { get; set; } = string.Empty; // BUY or SELL

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty; // LIMIT or MARKET

        [JsonProperty("volume")]
        public string Volume { get; set; } = string.Empty;

        [JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
        public string? Price { get; set; }
    }

    // Balance
    public class FameexBalance
    {
        [JsonPropertyName("asset")]
        public string Asset { get; set; } = string.Empty;

        [JsonPropertyName("free")]
        public string Free { get; set; } = string.Empty;  

        [JsonPropertyName("locked")]
        public string Locked { get; set; } = string.Empty;  
    }

    public class FameexAccountInfo
    {
        [JsonProperty("balances")]
        public List<FameexBalance> Balances { get; set; } = new();
    }

    // Error Response
    public class FameexErrorResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("msg")]
        public string Message { get; set; } = string.Empty;
    }
}