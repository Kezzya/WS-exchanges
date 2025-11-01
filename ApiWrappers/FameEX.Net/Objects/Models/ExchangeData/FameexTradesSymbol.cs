using Fameex.Net.Objects.Models.ExchangeData;
using Newtonsoft.Json;

namespace FameEX.Net.Objects.Models.ExchangeData;

public class FameexTradesSymbol : FameexSpotSymbol
{
    [JsonProperty("is_allow")]
    public int IsAllow { get; set; }
}