using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.Account
{
    public class ChainApexAccountInfo
    {
        [JsonProperty("balances")]
        public List<ChainApexBalance> Balances { get; set; } = new();
    }

    public class ChainApexBalance
    {
        [JsonProperty("asset")]
        public string Asset { get; set; } = string.Empty;

        [JsonProperty("free")]
        public string Free { get; set; } = string.Empty;

        [JsonProperty("locked")]
        public string Locked { get; set; } = string.Empty;

        [JsonProperty("total")]
        public string Total { get; set; } = string.Empty;

        [JsonProperty("available")]
        public string Available { get; set; } = string.Empty;
    }
}
