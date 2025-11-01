using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.ExchangeData;

public class DigifinexMarket
{
    [JsonProperty("volume_precision")]
    public int VolumePrecision { get; set; }

    [JsonProperty("price_precision")]
    public int PricePrecision { get; set; }

    [JsonProperty("market")]
    public string Market { get; set; } = string.Empty;

    [JsonProperty("min_amount")]
    public decimal MinAmount { get; set; }

    [JsonProperty("min_volume")]
    public decimal MinVolume { get; set; }
}