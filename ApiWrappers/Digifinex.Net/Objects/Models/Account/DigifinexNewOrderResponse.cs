using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexNewOrderResponse
{
    [JsonProperty("order_id")]
    public string OrderId { get; set; } = string.Empty;
}