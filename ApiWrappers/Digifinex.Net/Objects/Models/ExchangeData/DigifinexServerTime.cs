using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.ExchangeData;

public class DigifinexServerTime
{
    [JsonProperty("server_time")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime ServerTime { get; set; }

    [JsonProperty("code")]
    public int Code { get; set; }
}