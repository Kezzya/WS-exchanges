using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.ExchangeData
{
    public class ChainApexTicker
    {
        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("high")]
        public string High { get; set; } = string.Empty;

        [JsonProperty("low")]
        public string Low { get; set; } = string.Empty;

        [JsonProperty("last")]
        public string Last { get; set; } = string.Empty;

        [JsonProperty("vol")]
        public string Volume { get; set; } = string.Empty;

        [JsonProperty("amount")]
        public string Amount { get; set; } = string.Empty;

        [JsonProperty("buy")]
        public string Buy { get; set; } = string.Empty;

        [JsonProperty("sell")]
        public string Sell { get; set; } = string.Empty;

        [JsonProperty("rose")]
        public string Rose { get; set; } = string.Empty;
    }
}
