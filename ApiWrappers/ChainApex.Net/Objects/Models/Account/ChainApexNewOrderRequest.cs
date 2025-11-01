using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.Account
{
    public class ChainApexNewOrderRequest
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("side")]
        public string Side { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("volume")]
        public string Volume { get; set; } = string.Empty;

        [JsonProperty("price")]
        public string? Price { get; set; }
    }
}
