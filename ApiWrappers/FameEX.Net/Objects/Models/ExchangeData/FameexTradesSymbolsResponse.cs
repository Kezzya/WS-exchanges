using Fameex.Net.Objects.Models.ExchangeData;
using Newtonsoft.Json;

namespace FameEX.Net.Objects.Models.ExchangeData;

public class FameexTradesSymbolsResponse
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("symbol_list")]
    public List<FameexTradesSymbol> SymbolList { get; set; } = new();
}