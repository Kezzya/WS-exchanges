using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexFinanceLog
{
    [JsonProperty("time")]
    public long Time { get; set; }

    [JsonProperty("num")]
    public decimal Num { get; set; }

    [JsonProperty("balance")]
    public decimal Balance { get; set; }

    [JsonProperty("currency_mark")]
    public string CurrencyMark { get; set; } = string.Empty;

    [JsonProperty("type")]
    public int Type { get; set; }
}