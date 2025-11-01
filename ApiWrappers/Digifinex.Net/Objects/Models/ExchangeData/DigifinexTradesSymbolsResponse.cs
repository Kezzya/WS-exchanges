using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.ExchangeData;

public class DigifinexTradesSymbolsResponse
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("symbol_list")]
    public List<DigifinexTradesSymbol> SymbolList { get; set; } = new();
}