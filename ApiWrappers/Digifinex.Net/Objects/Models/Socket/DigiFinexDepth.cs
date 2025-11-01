using Digifinex.Net.Objects.Models.ExchangeData;
using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Socket;

public class DigifinexDepth
{
    [JsonProperty("asks")]
    public List<DigifinexOrderBookEntry> Asks { get; set; } 

    [JsonProperty("bids")]
    public List<DigifinexOrderBookEntry> Bids { get; set; }
}