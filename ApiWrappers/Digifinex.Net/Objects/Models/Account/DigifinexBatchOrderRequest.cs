using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexBatchOrderRequest
{
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("post_only")]
    public int PostOnly { get; set; }
}