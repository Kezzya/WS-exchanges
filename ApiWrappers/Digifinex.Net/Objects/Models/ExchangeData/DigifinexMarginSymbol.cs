using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.ExchangeData;

public class DigifinexMarginSymbol : DigifinexSpotSymbol
{
    [JsonProperty("liquidation_rate")]
    public decimal LiquidationRate { get; set; }
}