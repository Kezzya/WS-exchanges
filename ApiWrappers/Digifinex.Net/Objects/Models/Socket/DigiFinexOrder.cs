using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Socket
{
    public class DigifinexOrder
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("filled")]
        public decimal Filled { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("mode")]
        public DigifinexTradingMode Mode { get; set; }

        [JsonProperty("notional")]
        public decimal Notional { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("price_avg")]
        public decimal PriceAvg { get; set; }

        [JsonProperty("side")]
        public DigifinexOrderSide Side { get; set; }

        [JsonProperty("status")]
        public DigifinexOrderStatus Status { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("timestamp")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }

        [JsonProperty("type")]
        public DigifinexOrderType Type { get; set; }
    }
}