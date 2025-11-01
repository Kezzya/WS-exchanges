using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.ExchangeData;

public class DigifinexPing
{
    [JsonProperty("msg")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("code")]
    public int Code { get; set; }
}