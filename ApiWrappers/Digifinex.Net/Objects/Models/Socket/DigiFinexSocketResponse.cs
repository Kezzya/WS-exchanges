using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Socket;

public class DigifinexSocketResponse<T>
{
    [JsonProperty("error")]
    public object? Error { get; set; }

    [JsonProperty("result")]
    public T Result { get; set; } = default!;

    [JsonProperty("id")]
    public int? Id { get; set; }
}