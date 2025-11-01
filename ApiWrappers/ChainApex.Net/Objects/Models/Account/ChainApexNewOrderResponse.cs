using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.Account
{
    public class ChainApexNewOrderResponse
    {
        [JsonProperty("orderId")]
        public object OrderId { get; set; } = string.Empty;

        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("side")]
        public string Side { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("origQty")]
        public decimal OrigQty { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("executedQty")]
        public decimal ExecutedQty { get; set; }

        [JsonProperty("avgPrice")]
        public decimal AvgPrice { get; set; }

        [JsonProperty("transactTime")]
        public long TransactTime { get; set; }

        [JsonProperty("clientOrderId")]
        public string? ClientOrderId { get; set; }

        [JsonProperty("timeInForce")]
        public string? TimeInForce { get; set; }

        [JsonProperty("fills")]
        public List<ChainApexOrderFill>? Fills { get; set; }

        [JsonProperty("commission")]
        public decimal Commission { get; set; }

        [JsonProperty("commissionAsset")]
        public string? CommissionAsset { get; set; }
    }

    public class ChainApexOrderFill
    {
        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("qty")]
        public decimal Qty { get; set; }

        [JsonProperty("commission")]
        public decimal Commission { get; set; }

        [JsonProperty("commissionAsset")]
        public string? CommissionAsset { get; set; }

        [JsonProperty("tradeId")]
        public long TradeId { get; set; }
    }
}
