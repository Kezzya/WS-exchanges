using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;

namespace Fameex.Net.Objects.Models.ExchangeData;

public class FameexOrderBook
{
    [JsonProperty("bids")]
    public List<FameexOrderBookEntry> Bids { get; set; } = new();

    [JsonProperty("asks")]
    public List<FameexOrderBookEntry> Asks { get; set; } = new();

    [JsonProperty("date")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Date { get; set; }

    [JsonProperty("code")]
    public int Code { get; set; }
}