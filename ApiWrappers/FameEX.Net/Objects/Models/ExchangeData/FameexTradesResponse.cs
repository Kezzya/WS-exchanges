using CryptoExchange.Net.Converters.JsonNet;
using FameEX.Net.Models;
using Newtonsoft.Json;

namespace Fameex.Net.Objects.Models.ExchangeData;

public class FameexTradesResponse
{
    [JsonProperty("data")]
    public List<FameexTrade> Data { get; set; } = new();

    [JsonProperty("date")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Date { get; set; }

    [JsonProperty("code")]
    public int Code { get; set; }
}