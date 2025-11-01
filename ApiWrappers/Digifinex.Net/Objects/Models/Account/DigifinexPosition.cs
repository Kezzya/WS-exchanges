using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexPosition
{
    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("leverage_ratio")]
    public string LeverageRatio { get; set; } = string.Empty;

    [JsonProperty("side")]
    public string Side { get; set; } = string.Empty;

    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("entry_price")]
    public decimal EntryPrice { get; set; }

    [JsonProperty("unrealized_pnl")]
    public decimal UnrealizedPnl { get; set; }

    [JsonProperty("liquidation_price")]
    public decimal LiquidationPrice { get; set; }

    [JsonProperty("liquidation_rate")]
    public decimal LiquidationRate { get; set; }
}