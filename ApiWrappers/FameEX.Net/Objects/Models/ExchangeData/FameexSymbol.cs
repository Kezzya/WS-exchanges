using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Fameex.Net.Objects.Models.ExchangeData;

public class FameexSymbol
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty; // "btcusdt"

    [JsonPropertyName("baseAsset")]
    public string BaseAsset { get; set; } = string.Empty; // "BTC"

    [JsonPropertyName("quoteAsset")]
    public string QuoteAsset { get; set; } = string.Empty; // "USDT"

    [JsonPropertyName("quantityPrecision")]
    public int QuantityPrecision { get; set; } // 8

    [JsonPropertyName("pricePrecision")]
    public int PricePrecision { get; set; } // 2

    [JsonPropertyName("limitVolumeMin")]
    public decimal LimitVolumeMin { get; set; } // 0.0001 — минимальный объем

    [JsonPropertyName("marketBuyMin")]
    public decimal MarketBuyMin { get; set; } // 0.0001

    [JsonPropertyName("marketSellMin")]
    public decimal MarketSellMin { get; set; } // 0.0001

    [JsonPropertyName("limitPriceMin")]
    public decimal LimitPriceMin { get; set; } // 0.001 — минимальная цена
}