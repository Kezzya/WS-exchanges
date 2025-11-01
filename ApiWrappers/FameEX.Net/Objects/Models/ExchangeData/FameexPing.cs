using Newtonsoft.Json;

namespace Fameex.Net.Objects.Models.ExchangeData;

public class FameexPing
{
    [JsonProperty("msg")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("code")]
    public int Code { get; set; }
}