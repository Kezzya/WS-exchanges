using FameEX.Net.Models;
using Newtonsoft.Json;

namespace Fameex.Net.Objects.Models.ExchangeData;

public class FameexKlineResponse
{
    [JsonProperty("data")]
    public List<FameexKline> Data { get; set; } = new();

    [JsonProperty("code")]
    public int Code { get; set; }
}