using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;

namespace FameEX.Net.Objects.Models.Account
{
    public class FameexWithdrawHistory
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonProperty("txid")]
        public string TransactionId { get; set; } = string.Empty;

        [JsonProperty("chain")]
        public string Chain { get; set; } = string.Empty;

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; } = string.Empty;

        [JsonProperty("memo")]
        public string Memo { get; set; } = string.Empty;

        [JsonProperty("fee")]
        public decimal Fee { get; set; }

        [JsonProperty("status")]
        public FameexWithdrawState Status { get; set; }

        [JsonProperty("created_at")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("updated_at")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime? UpdatedDate { get; set; }
    }
}