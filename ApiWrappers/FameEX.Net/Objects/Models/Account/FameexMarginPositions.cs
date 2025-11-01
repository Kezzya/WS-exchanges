using Newtonsoft.Json;

namespace FameEX.Net.Objects.Models.Account
{
    public class FameexMarginPositions
    {
        [JsonProperty("margin")]
        public string Margin { get; set; } = string.Empty;

        [JsonProperty("margin_rate")]
        public string MarginRate { get; set; } = string.Empty;

        [JsonProperty("unrealized_pnl")]
        public string UnrealizedPnl { get; set; } = string.Empty;

        [JsonProperty("positions")]
        public List<FameexPosition> Positions { get; set; } = new();
    }
}