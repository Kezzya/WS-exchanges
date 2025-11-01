using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;

namespace FameEX.Net.Objects.Models.Account
{
    public class FameexOrder
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("order_id")]
        public string OrderId { get; set; } = string.Empty;

        [JsonProperty("created_at")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("updated_at")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime? UpdatedDate { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("filled_amount")]
        public decimal FilledAmount { get; set; }

        [JsonProperty("avg_price")]
        public decimal? AvgPrice { get; set; }

        [JsonProperty("state")]
        public FameexOrderStatus Status { get; set; }

        [JsonProperty("type")]
        public FameexOrderType Type { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; } = string.Empty;
    }
    public class FameexOrderDetail : FameexOrder
    {
        [JsonProperty("detail")]
        public FameexTradeDetail Detail { get; set; } = new();
    }
}