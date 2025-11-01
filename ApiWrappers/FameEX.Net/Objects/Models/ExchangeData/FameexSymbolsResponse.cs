using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Fameex.Net.Objects.Models.ExchangeData;

public class FameexSymbolsResponse
{
    [JsonPropertyName("symbols")]
    public List<FameexSymbol> Symbols { get; set; } = new();
}