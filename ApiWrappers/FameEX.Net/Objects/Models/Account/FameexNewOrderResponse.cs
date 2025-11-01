using Newtonsoft.Json;

namespace FameEX.Net.Objects.Models.Account
{
    public class FameexNewOrderResponse
    {
        [JsonProperty("order_id")]
        public string OrderId { get; set; } = string.Empty;
    }
}