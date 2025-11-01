using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexDepositHistory
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonProperty("hashId")]
    public string HashId { get; set; } = string.Empty;

    [JsonProperty("chain")]
    public string Chain { get; set; } = string.Empty;

    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("address")]
    public string Address { get; set; } = string.Empty;

    [JsonProperty("state")]
    public DigifinexDepositState State { get; set; }

    [JsonProperty("created_date")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreatedDate { get; set; } 

    [JsonProperty("update_date")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime UpdateDate { get; set; }
}