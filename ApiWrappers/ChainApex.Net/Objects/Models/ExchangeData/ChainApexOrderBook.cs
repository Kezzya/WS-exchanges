using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.ExchangeData
{
    public class ChainApexOrderBook
    {
        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("bids")]
        public List<List<string>> Bids { get; set; } = new();

        [JsonProperty("asks")]
        public List<List<string>> Asks { get; set; } = new();
    }

    public class ChainApexOrderBookEntry
    {
        public string Price { get; set; } = string.Empty;
        public string Quantity { get; set; } = string.Empty;
    }
}
