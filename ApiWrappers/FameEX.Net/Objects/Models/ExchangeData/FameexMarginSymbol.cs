using Newtonsoft.Json;

namespace Fameex.Net.Objects.Models.ExchangeData;

public class FameexMarginSymbol : FameexSpotSymbol
{
    [JsonProperty("liquidation_rate")]
    public decimal LiquidationRate { get; set; }
}