using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Socket;

public class DigifinexTrade
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("time")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Time { get; set; }

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;
}