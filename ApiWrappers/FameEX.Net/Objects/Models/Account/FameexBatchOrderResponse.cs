using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace FameEX.Net.Objects.Models.Account
{
    public class FameexBatchOrderResponse
    {
        [JsonPropertyName("success")]
        public long[] Success { get; set; } = Array.Empty<long>();

        [JsonPropertyName("failed")]
        public long[] Failed { get; set; } = Array.Empty<long>();
        public string[] OrderIds => Success?.Select(x => x.ToString()).ToArray() ?? Array.Empty<string>();

    }
    public class FameexCancelOrderResponse
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("orderId")]
        public string[] OrderId { get; set; } = Array.Empty<string>();  

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty; // "PENDING_CANCEL"
        public string[] Success => OrderId;
    }
}