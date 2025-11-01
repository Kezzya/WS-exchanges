using Newtonsoft.Json;

namespace Fameex.Net.Objects.Models.ExchangeData;

public class FameexSpotSymbolsResponse
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("symbol_list")]
    public List<FameexSpotSymbol> SymbolList { get; set; } = new();
}