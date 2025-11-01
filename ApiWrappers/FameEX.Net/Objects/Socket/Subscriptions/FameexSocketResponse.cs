using CryptoExchange.Net.Objects;
using Newtonsoft.Json;

namespace FameEX.Net.Objects.Socket
{
    public class FameexSocketResponse<T>
    {
        public string Method { get; set; }
        public T Result { get; set; }
        [JsonProperty("error")]
        public object? Error { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; } = string.Empty;

        [JsonProperty("ts")]
        public long Timestamp { get; set; }

        [JsonProperty("tick")]
        public T Tick { get; set; } = default!;
    }

    public class FameexSocketUpdate<T> : FameexSocketResponse<T>
    {
        [JsonProperty("params")]
        public T Params { get; set; } = default!;
    }

    public class FameexSubscriptionResult
    {
        public string Result { get; set; }
        public int Id { get; set; }
    }
}