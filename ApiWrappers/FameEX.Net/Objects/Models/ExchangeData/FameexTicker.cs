using Newtonsoft.Json;

namespace Fameex.Net.Objects.Models.ExchangeData;

public class FameexTicker
{
    [JsonProperty("vol")]
    public decimal Volume { get; set; }

    [JsonProperty("change")]
    public decimal Change { get; set; }

    [JsonProperty("base_vol")]
    public decimal BaseVolume { get; set; }

    [JsonProperty("sell")]
    public decimal Sell { get; set; }

    [JsonProperty("last")]
    public decimal Last { get; set; }

    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("low")]
    public decimal Low { get; set; }

    [JsonProperty("buy")]
    public decimal Buy { get; set; }

    [JsonProperty("high")]
    public decimal High { get; set; }
}