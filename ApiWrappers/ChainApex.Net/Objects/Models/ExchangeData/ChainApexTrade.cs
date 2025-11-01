using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.ExchangeData
{
    public class ChainApexTrade
    {
        [JsonProperty("side")]
        public string Side { get; set; } = string.Empty;

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("qty")]
        public decimal Qty { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("tradeId")]
        public long TradeId { get; set; }

        [JsonProperty("isBuyerMaker")]
        public bool IsBuyerMaker { get; set; }

        [JsonProperty("commission")]
        public decimal Commission { get; set; }

        [JsonProperty("commissionAsset")]
        public string? CommissionAsset { get; set; }
    }
}
