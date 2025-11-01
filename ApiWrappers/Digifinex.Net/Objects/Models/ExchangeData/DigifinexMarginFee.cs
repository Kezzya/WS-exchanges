using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.ExchangeData;

public class DigifinexMarginFee
{
    [JsonProperty("currency_mark")]
    public string CurrencyMark { get; set; } = string.Empty;

    [JsonProperty("level")]
    public int Level { get; set; }

    [JsonProperty("range")]
    public string Range { get; set; } = string.Empty;

    [JsonProperty("loan_fees")]
    public decimal LoanFees { get; set; }
}