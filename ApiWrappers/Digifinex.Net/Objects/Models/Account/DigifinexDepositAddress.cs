using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexDepositAddress
{
    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonProperty("address")]
    public string Address { get; set; } = string.Empty;

    [JsonProperty("addressTag")]
    public string AddressTag { get; set; } = string.Empty;

    [JsonProperty("chain")]
    public string Chain { get; set; } = string.Empty;
}