using Newtonsoft.Json;

namespace Fameex.Net.Objects.Models.ExchangeData;

public class FameexMarginSymbolsResponse
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("symbol_list")]
    public List<FameexMarginSymbol> SymbolList { get; set; } = new();
}