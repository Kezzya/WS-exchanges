using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.Account
{
    public class ChainApexMyTrade
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("side")]
        public string Side { get; set; } = string.Empty;

        [JsonProperty("fee")]
        public string Fee { get; set; } = string.Empty;

        [JsonProperty("isMaker")]
        public bool IsMaker { get; set; }

        [JsonProperty("isBuyer")]
        public bool IsBuyer { get; set; }

        [JsonProperty("bidId")]
        public long BidId { get; set; }

        [JsonProperty("bidUserId")]
        public int BidUserId { get; set; }

        [JsonProperty("feeCoin")]
        public string FeeCoin { get; set; } = string.Empty;

        [JsonProperty("price")]
        public string Price { get; set; } = string.Empty;

        [JsonProperty("qty")]
        public string Qty { get; set; } = string.Empty;

        [JsonProperty("askId")]
        public long AskId { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("isSelf")]
        public bool IsSelf { get; set; }

        [JsonProperty("askUserId")]
        public int AskUserId { get; set; }

        [JsonProperty("orderId")]
        public long OrderId { get; set; }

        [JsonProperty("commission")]
        public string Commission { get; set; } = string.Empty;

        [JsonProperty("commissionAsset")]
        public string? CommissionAsset { get; set; }
    }
}
