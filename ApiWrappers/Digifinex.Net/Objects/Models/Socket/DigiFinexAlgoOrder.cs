using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Socket;

public class DigifinexAlgoOrder
{
    [JsonProperty("algo_price")]
    public string AlgoPrice { get; set; } = string.Empty;

    [JsonProperty("amount")]
    public string Amount { get; set; } = string.Empty;

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("mode")]
    public DigifinexTradingMode Mode { get; set; }

    [JsonProperty("side")]
    public string Side { get; set; } = string.Empty;

    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }

    [JsonProperty("trigger_price")]
    public string TriggerPrice { get; set; } = string.Empty;
}