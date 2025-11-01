using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.Socket
{
    /// <summary>
    /// ChainStream WebSocket request format (alternative protocol)
    /// </summary>
    public class ChainStreamRequest
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("channel")]
        public string? Channel { get; set; }

        [JsonProperty("token")]
        public string? Token { get; set; }
    }

    /// <summary>
    /// ChainStream WebSocket response format
    /// </summary>
    public class ChainStreamResponse<T>
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("channel")]
        public string? Channel { get; set; }

        [JsonProperty("data")]
        public T? Data { get; set; }

        [JsonProperty("timestamp")]
        public long? Timestamp { get; set; }
    }
}

