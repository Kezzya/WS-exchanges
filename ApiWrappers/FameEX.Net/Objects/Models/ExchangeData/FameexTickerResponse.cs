using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Fameex.Net.Objects.Models.ExchangeData;

public class FameexTickerResponse
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; } // Transaction Amount

    [JsonPropertyName("high")]
    public decimal High { get; set; }   // Highest price

    [JsonPropertyName("vol")]
    public decimal Volume { get; set; } // Trading volume

    [JsonPropertyName("last")]
    public decimal LastPrice { get; set; } // Latest trade price

    [JsonPropertyName("low")]
    public decimal Low { get; set; }    // Lowest price

    [JsonPropertyName("buy")]
    public decimal BidPrice { get; set; } // Bid price (optional, если нужно)

    [JsonPropertyName("sell")]
    public decimal AskPrice { get; set; } // Ask price (optional, если нужно)

    [JsonPropertyName("rose")]
    public string Change24H { get; set; } = string.Empty; // ✅ Строка! "+0.05"

    [JsonPropertyName("time")]
    public long Timestamp { get; set; }
}