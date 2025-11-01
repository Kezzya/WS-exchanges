using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.ExchangeData;

public class DigifinexKlineResponse
{
    [JsonProperty("data")]
    public List<DigifinexKline> Data { get; set; } = new();

    [JsonProperty("code")]
    public int Code { get; set; }
}