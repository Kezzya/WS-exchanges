using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexTrade
{
    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("order_id")]
    public string OrderId { get; set; } = string.Empty;

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("fee")]
    public decimal Fee { get; set; }

    [JsonProperty("fee_currency")]
    public string FeeCurrency { get; set; } = string.Empty;

    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }

    [JsonProperty("side")]
    public string Side { get; set; } = string.Empty;

    [JsonProperty("is_maker")]
    public bool IsMaker { get; set; }
}