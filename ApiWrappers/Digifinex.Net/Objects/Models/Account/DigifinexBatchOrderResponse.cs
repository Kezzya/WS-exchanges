using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexBatchOrderResponse
{
    [JsonProperty("order_ids")]
    public List<string> OrderIds { get; set; } = new();
}