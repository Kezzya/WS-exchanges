using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.Socket
{
    public class ChainApexSocketTradeData
    {
        [JsonProperty("data")]
        public List<ChainApexSocketTrade> Data { get; set; } = new();
    }

    public class ChainApexSocketTrade
    {
        [JsonProperty("side")]
        public string Side { get; set; } = string.Empty;

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("vol")]
        public decimal Volume { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("ds")]
        public string? Timestamp { get; set; }
    }
}

