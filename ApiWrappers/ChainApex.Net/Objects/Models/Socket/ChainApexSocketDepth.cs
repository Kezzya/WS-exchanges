using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.Socket
{
    public class ChainApexSocketDepth
    {
        [JsonProperty("asks")]
        public List<List<decimal>> Asks { get; set; } = new();

        [JsonProperty("buys")]
        public List<List<decimal>> Buys { get; set; } = new();
    }
}

