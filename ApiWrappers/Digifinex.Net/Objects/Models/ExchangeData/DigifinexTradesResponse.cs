using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.ExchangeData;

public class DigifinexTradesResponse
{
    [JsonProperty("data")]
    public List<DigifinexTrade> Data { get; set; } = new();

    [JsonProperty("date")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Date { get; set; }

    [JsonProperty("code")]
    public int Code { get; set; }
}