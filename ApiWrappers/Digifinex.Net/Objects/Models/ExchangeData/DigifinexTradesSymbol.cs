using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.ExchangeData;

public class DigifinexTradesSymbol : DigifinexSpotSymbol
{
    [JsonProperty("is_allow")]
    public int IsAllow { get; set; }
}