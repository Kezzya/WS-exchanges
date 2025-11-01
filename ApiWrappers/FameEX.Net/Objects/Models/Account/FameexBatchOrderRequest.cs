using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace FameEX.Net.Objects.Models.Account
{
    public class FameexBatchOrderRequest
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("orderIds")]
        public long[] OrderIds { get; set; } = Array.Empty<long>();
    }
}