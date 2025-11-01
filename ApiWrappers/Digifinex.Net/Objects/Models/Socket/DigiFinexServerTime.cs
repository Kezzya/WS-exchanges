using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Socket;

public class DigifinexServerTime
{
    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }
}