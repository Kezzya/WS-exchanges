using Newtonsoft.Json;

namespace FameEX.Net.Objects.Models.Account
{
    public class FameexDepositAddress
    {
        [JsonProperty("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonProperty("address")]
        public string Address { get; set; } = string.Empty;

        [JsonProperty("memo")]
        public string Memo { get; set; } = string.Empty;

        [JsonProperty("chain")]
        public string Chain { get; set; } = string.Empty;
    }
}