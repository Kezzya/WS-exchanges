using Newtonsoft.Json;

namespace Fameex.Net.Objects.Models.ExchangeData;

public class FameexMarginFee
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