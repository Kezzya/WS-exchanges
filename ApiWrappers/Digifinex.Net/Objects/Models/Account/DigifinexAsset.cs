using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexAsset
{
    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonProperty("free")]
    public decimal Free { get; set; }

    [JsonProperty("total")]
    public decimal Total { get; set; }
}