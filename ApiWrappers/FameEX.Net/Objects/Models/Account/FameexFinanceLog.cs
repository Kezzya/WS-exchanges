using Newtonsoft.Json;

namespace FameEX.Net.Objects.Models.Account
{
    public class FameexFinanceLog
    {
        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("balance")]
        public decimal Balance { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonProperty("type")]
        public int Type { get; set; }
    }
}