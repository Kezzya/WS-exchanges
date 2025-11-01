using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexTradeDetail
{
    [JsonProperty("tid")]
    public string Tid { get; set; } = string.Empty;

    [JsonProperty("date")]
    public long Date { get; set; }

    [JsonProperty("executed_amount")]
    public decimal ExecutedAmount { get; set; }

    [JsonProperty("executed_price")]
    public decimal ExecutedPrice { get; set; }
}