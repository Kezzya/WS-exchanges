using Newtonsoft.Json;

namespace FameEX.Net.Objects.Models.Account
{
    public class FameexMarginAsset : FameexAsset
    {
        [JsonProperty("valuation_rate")]
        public decimal ValuationRate { get; set; }
    }
}