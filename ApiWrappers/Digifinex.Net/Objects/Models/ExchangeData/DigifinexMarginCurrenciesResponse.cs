using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.ExchangeData;

public class DigifinexMarginCurrenciesResponse
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("funding_time")]
    public string FundingTime { get; set; } = string.Empty;

    [JsonProperty("currencys")]
    public List<string> Currencies { get; set; } = new();

    [JsonProperty("margin_fees")]
    public List<DigifinexMarginFee> MarginFees { get; set; } = new();
}