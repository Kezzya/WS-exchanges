using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Socket;

public class DigifinexSocketRequest
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("method")]
    public string Method { get; set; } = string.Empty;

    [JsonProperty("params")]
    public object[] Params { get; set; } = Array.Empty<object>();
}