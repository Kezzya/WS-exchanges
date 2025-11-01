using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;

namespace FameEX.Net.Objects.Models.Account
{
    public enum FameexDepositState
    {
        InDeposit = 1,
        ToBeConfirmed = 2,
        SuccessfullyDeposited = 3,
        Stopped = 4
    }
    public class FameexDepositHistory
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

        [JsonProperty("status")]
        public FameexDepositState Status { get; set; }

        [JsonProperty("created_at")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("updated_at")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime? UpdatedDate { get; set; }
    }
}