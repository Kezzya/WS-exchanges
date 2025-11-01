using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.ExchangeData;

public class DigifinexOrderBook
{
    [JsonProperty("bids")]
    public List<DigifinexOrderBookEntry> Bids { get; set; } = new();

    [JsonProperty("asks")]
    public List<DigifinexOrderBookEntry> Asks { get; set; } = new();

    [JsonProperty("date")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Date { get; set; }

    [JsonProperty("code")]
    public int Code { get; set; }
}