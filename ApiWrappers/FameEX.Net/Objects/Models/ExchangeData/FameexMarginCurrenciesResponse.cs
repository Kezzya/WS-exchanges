using Newtonsoft.Json;

namespace Fameex.Net.Objects.Models.ExchangeData;

public class FameexMarginCurrenciesResponse
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("funding_time")]
    public string FundingTime { get; set; } = string.Empty;

    [JsonProperty("currencys")]
    public List<string> Currencies { get; set; } = new();

    [JsonProperty("margin_fees")]
    public List<FameexMarginFee> MarginFees { get; set; } = new();
}