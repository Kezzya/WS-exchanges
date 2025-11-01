using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.ExchangeData;

public class DigifinexSpotSymbol
{
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("quote_asset")]
    public string QuoteAsset { get; set; } = string.Empty;

    [JsonProperty("base_asset")]
    public string BaseAsset { get; set; } = string.Empty;

    [JsonProperty("amount_precision")]
    public int AmountPrecision { get; set; }

    [JsonProperty("price_precision")]
    public int PricePrecision { get; set; }

    [JsonProperty("minimum_amount")]
    public decimal MinimumAmount { get; set; }

    [JsonProperty("minimum_value")]
    public decimal MinimumValue { get; set; }

    [JsonProperty("zone")]
    public string Zone { get; set; } = string.Empty;

    [JsonProperty("order_types")]
    public List<string> OrderTypes { get; set; } = new();
}