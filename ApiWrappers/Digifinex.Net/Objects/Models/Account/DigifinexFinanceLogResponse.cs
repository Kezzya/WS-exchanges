using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexFinanceLogResponse
{
    [JsonProperty("finance")]
    public List<DigifinexFinanceLog> Finance { get; set; } = new();

    [JsonProperty("total")]
    public string Total { get; set; } = string.Empty;
}