using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexMarginPositions
{
    [JsonProperty("margin")]
    public string Margin { get; set; } = string.Empty;

    [JsonProperty("margin_rate")]
    public string MarginRate { get; set; } = string.Empty;

    [JsonProperty("unrealized_pnl")]
    public string UnrealizedPnl { get; set; } = string.Empty;

    [JsonProperty("positions")]
    public List<DigifinexPosition> Positions { get; set; } = new();
}