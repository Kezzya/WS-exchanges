using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexMarginAssets
{
    [JsonProperty("total")]
    public decimal Total { get; set; }

    [JsonProperty("free")]
    public decimal Free { get; set; }

    [JsonProperty("unrealized_pnl")]
    public decimal UnrealizedPnl { get; set; }

    [JsonProperty("list")]
    public List<DigifinexMarginAsset> List { get; set; } = new();
}