using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexMarginAsset : DigifinexAsset
{
    [JsonProperty("valuation_rate")]
    public decimal ValuationRate { get; set; }
}