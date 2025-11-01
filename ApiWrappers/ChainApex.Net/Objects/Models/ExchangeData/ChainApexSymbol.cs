using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.ExchangeData
{
    public class ChainApexSymbol
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

    public class ChainApexSymbolsResponse
    {
        [JsonProperty("symbols")]
        public List<ChainApexSymbol> Symbols { get; set; } = new();
    }
}
