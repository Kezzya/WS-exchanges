using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexOrderDetail : DigifinexOrder
{
    [JsonProperty("detail")]
    public DigifinexTradeDetail Detail { get; set; } = new();
}