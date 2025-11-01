using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.ExchangeData;

public class DigifinexMarginSymbolsResponse
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("symbol_list")]
    public List<DigifinexMarginSymbol> SymbolList { get; set; } = new();
}