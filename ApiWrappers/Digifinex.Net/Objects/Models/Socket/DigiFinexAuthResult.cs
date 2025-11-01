using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Socket;

public class DigifinexAuthResult
{
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;
}