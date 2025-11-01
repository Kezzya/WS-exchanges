using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexCancelOrderResponse
{
    [JsonProperty("date")]
    public long Date { get; set; }

    [JsonProperty("success")]
    public List<string> Success { get; set; } = new();

    [JsonProperty("error")]
    public List<string> Error { get; set; } = new();
}