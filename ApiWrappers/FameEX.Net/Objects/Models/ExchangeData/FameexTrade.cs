using System.Text.Json.Serialization;

namespace FameEX.Net.Objects.Models.Account
{
    public class FameexTrade
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("side")]
        public string Side { get; set; } = string.Empty; // "BUY" или "SELL"

        [JsonPropertyName("fee")]
        public decimal Fee { get; set; }

        [JsonPropertyName("isMaker")]
        public bool IsMaker { get; set; }

        [JsonPropertyName("isBuyer")]
        public bool IsBuyer { get; set; }

        [JsonPropertyName("bidId")]
        public long BidId { get; set; }

        [JsonPropertyName("bidUserId")]
        public int BidUserId { get; set; }

        [JsonPropertyName("feeCoin")]
        public string FeeCoin { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("qty")]
        public decimal Qty { get; set; } // ← Объём сделки (не Amount!)

        [JsonPropertyName("askId")]
        public long AskId { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; } // ← Это int, а не string!

        [JsonPropertyName("time")]
        public long Time { get; set; } // ← Timestamp в миллисекундах

        [JsonPropertyName("isSelf")]
        public bool IsSelf { get; set; }

        [JsonPropertyName("askUserId")]
        public int AskUserId { get; set; }
    }
}