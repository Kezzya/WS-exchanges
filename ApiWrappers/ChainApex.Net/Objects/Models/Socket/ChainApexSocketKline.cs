using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.Socket
{
    public class ChainApexSocketKline
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("vol")]
        public decimal Volume { get; set; }

        [JsonProperty("open")]
        public decimal Open { get; set; }

        [JsonProperty("close")]
        public decimal Close { get; set; }

        [JsonProperty("high")]
        public decimal High { get; set; }

        [JsonProperty("low")]
        public decimal Low { get; set; }

        [JsonProperty("amount")]
        public decimal? Amount { get; set; }
    }
}

