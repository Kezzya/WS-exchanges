using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Socket;

public class DigifinexSocketUpdate<T>
{
    [JsonProperty("method")]
    public string Method { get; set; } = string.Empty;

    [JsonProperty("params")]
    public T Params { get; set; } = default!;

    [JsonProperty("id")]
    public object? Id { get; set; }
}