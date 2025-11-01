using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Socket;

public class DigifinexPong
{
    [JsonProperty("result")]
    public string Result { get; set; } = string.Empty;
}