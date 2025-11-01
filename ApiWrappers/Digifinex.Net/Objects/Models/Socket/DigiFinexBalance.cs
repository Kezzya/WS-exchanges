using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Socket;

public class DigifinexBalance
{
    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonProperty("free")]
    public string Free { get; set; } = string.Empty;

    [JsonProperty("total")]
    public string Total { get; set; } = string.Empty;

    [JsonProperty("used")]
    public string Used { get; set; } = string.Empty;
}