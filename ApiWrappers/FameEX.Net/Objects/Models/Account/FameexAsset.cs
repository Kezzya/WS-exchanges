using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace FameEX.Net.Objects.Models.Account
{
    public class FameexAsset
    {
        [JsonPropertyName("asset")]
        public string Asset { get; set; } = string.Empty;

        [JsonPropertyName("free")]
        public string Free { get; set; } = string.Empty;

        [JsonPropertyName("locked")]
        public string Locked { get; set; } = string.Empty;
    }
    public class FameexMarginAssets
    {
        [JsonProperty("total")]
        public decimal Total { get; set; }

        [JsonProperty("free")]
        public decimal Free { get; set; }

        [JsonProperty("unrealized_pnl")]
        public decimal UnrealizedPnl { get; set; }

        [JsonProperty("list")]
        public List<FameexMarginAsset> List { get; set; } = new();
    }
}