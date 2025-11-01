using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.ExchangeData
{
    public class ChainApexKline
    {
        [JsonProperty("idx")]
        public long Idx { get; set; }

        [JsonProperty("open")]
        public string Open { get; set; } = string.Empty;

        [JsonProperty("high")]
        public string High { get; set; } = string.Empty;

        [JsonProperty("low")]
        public string Low { get; set; } = string.Empty;

        [JsonProperty("close")]
        public string Close { get; set; } = string.Empty;

        [JsonProperty("vol")]
        public string Volume { get; set; } = string.Empty;

        [JsonProperty("amount")]
        public string Amount { get; set; } = string.Empty;
    }
}
