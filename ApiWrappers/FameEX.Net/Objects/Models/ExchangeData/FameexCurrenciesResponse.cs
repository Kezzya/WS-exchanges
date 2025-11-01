using Fameex.Net.Objects.Models.ExchangeData;
using Newtonsoft.Json;

namespace FameEX.Net.Objects.Models.ExchangeData;

public class FameexCurrenciesResponse
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("data")]
    public List<FameexCurrency> Data { get; set; } = new();
}