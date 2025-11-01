using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.Socket
{
    public class ChainApexSocketResponse<T>
    {
        [JsonProperty("channel")]
        public string Channel { get; set; } = string.Empty;

        [JsonProperty("ts")]
        public long Timestamp { get; set; }

        [JsonProperty("tick")]
        public T Tick { get; set; }

        [JsonProperty("event_rep")]
        public string? EventRep { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("data")]
        public T? Data { get; set; }
    }

    public class ChainApexPingPong
    {
        [JsonProperty("ping")]
        public long? Ping { get; set; }

        [JsonProperty("pong")]
        public long? Pong { get; set; }
    }
}

